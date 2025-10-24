# QR Transfer Protocol

## Frame layout
Each QR frame encodes a compact binary packet. The sender reserves the first six bytes for transport metadata and uses the remaining bytes for the payload.

| Offset | Size | Description |
| --- | --- | --- |
| 0 | 1 | Flags: bit 7 = 1 for metadata frames, 0 for file data. Bits 0–3 store the file slot (0–15). Bits 4–6 are reserved and must be zero. |
| 1 | 2 | Little-endian offset of the payload within the logical stream (metadata or data) for the selected file. |
| 3 | 1 | Payload length in bytes (0–255). |
| 4 | 2 | Total length of the logical stream in bytes (0–65535). |
| 6 | _N_ | Raw payload bytes. |

The sender always broadcasts metadata packets for a file before any data packets. Receivers should reconstruct the metadata and use it to prepare for the upcoming data stream.

## Metadata payload
Metadata frames describe the file so receivers can display names and verify integrity. The metadata stream is structured as follows (little-endian unless noted otherwise):

| Offset | Size | Description |
| --- | --- | --- |
| 0 | 1 | Metadata format version (currently `2`). |
| 1 | 1 | UTF-8 file name length in bytes. |
| 2 | _N_ | UTF-8 encoded file name (`N` bytes). |
| 2 + _N_ | 2 | File size in bytes (0–65,535). |
| 4 + _N_ | 1 | Data chunk size in bytes used for transmission. |
| 5 + _N_ | 1 | Error-correction level as an uppercase ASCII code (`L`, `M`, `Q`, `H`) or `0` when unknown. |
| 6 + _N_ | 2 | Block size for per-block checksums (the sender writes `256`). |
| 8 + _N_ | 2 | Number of checksum entries. |
| 10 + _N_ | 4 | CRC32 checksum of the entire file. |
| 14 + _N_ | `4 × count` | CRC32 checksums for each block of the declared size. |

An empty file still yields a metadata stream (with zero checksum entries) and a data frame whose payload length is zero. Per-block checksums let the receiver detect corrupt retransmissions without waiting for the full file, while the file-level CRC32 verifies the final reconstructed blob before exposing a download link.

## Size limits
* Up to 16 files can be queued simultaneously. Each file is assigned a slot index that is reused across retransmissions.
* File size is limited to 65,535 bytes so that offsets fit into two bytes.
* Payload length per frame is capped at 255 bytes to fit the one-byte length field.

## Chunk sizing rules
`QrCapacityCatalog` exposes the maximum number of bytes that can be encoded for every combination of QR version and error-correction level. The effective payload size for a frame is:

```
min(capacity - 6, 255)
```

The subtraction accounts for the 6-byte transport header. If the selected QR configuration cannot fit the header, the sender disables transmission.

## Transmission loop
1. The operator enqueues files via drag-and-drop or the file picker. `QrChunkBuilder` splits every file into fixed-size slices (or a smaller tail chunk) and creates both metadata and data packets.
2. When the Start button is pressed, the sender iterates over the queue. For each file it emits the metadata frames once, followed by the data frames in offset order. The UI highlights the current file and shows progress in chunks.
3. Once the data stream reaches the declared total length, the file is marked as completed and the sender advances to the next pending file. If repeat mode is enabled, finished files reset and the loop starts over.
4. Frame timing is controlled by the configured frame duration. The sender waits for the specified number of milliseconds between frames while it remains running. Pausing the transmission stops frame generation without clearing progress.
5. Restart clears chunk indices for every queued file, resets the current pointer, and removes the QR preview so the operator can start a fresh cycle.

Receivers assemble frames per file slot, merging metadata and data streams separately. Missing or corrupt data blocks can be detected by comparing the reconstructed file against the advertised block checksums. The metadata flag allows receivers to handle descriptive information and payload bytes independently.
