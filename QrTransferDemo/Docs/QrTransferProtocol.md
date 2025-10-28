# QR Transfer Protocol

## Frame layout

Each QR frame encodes a compact binary packet. The sender reserves the first six bytes for transport metadata and uses the remaini
ng bytes for the payload.

| Offset | Size | Description |
| --- | --- | --- |
| 0 | 1 | Flags: bit 7 = 1 for metadata frames, 0 for file data. Bits 0–3 store the file slot (0–15). Bits 4–6 are reserved and
 must be zero. |
| 1 | 1 | Payload length in bytes (0–255). |
| 2 | 2 | Total length of the logical stream in bytes (0–65535). |
| 4 | 2 | Little-endian offset of the payload within the logical stream (data or metadata) for the selected file. |
| 6 | _N_ | Raw payload bytes. |

The sender always broadcasts metadata packets for a file before any data packets. Receivers should reconstruct the metadata and u
se it to prepare for the upcoming data stream.

## Metadata payload
Metadata frames describe the file so receivers can display names and verify integrity. The metadata stream is structured as foll
ows:

| Offset | Size | Description |
| --- | --- | --- |
| 0 | 1 | UTF-8 file name length in bytes. |
| 1 | _N_ | UTF-8 encoded file name (N bytes). |
| 1 + _N_ | 2 | Little-endian file size in bytes (0–65535). |
| 3 + _N_ | 1 | Chunk size used for payload slicing (in bytes). |
| 4 + _N_ | 1 | QR correction level as an ASCII character (`L`, `M`, `Q`, `H`) or `0` if unspecified. |
| 5 + _N_ | 2 | Metadata block size (defaults to `256`). |
| 7 + _N_ | 2 | Number of checksum entries. |
| 9 + _N_ | 4 | CRC-32 of the entire file (0 if omitted). |
| 13 + _N_ | `4 × count` | CRC-32 for each consecutive block of `blockSize` bytes. |

An empty file still yields a metadata stream (with zero checksums) and a data frame whose payload length is zero.

## Size limits
* Up to 16 files can be queued simultaneously. Each file is assigned a slot index that is reused across retransmissions.
* File size is limited to 65,535 bytes so that offsets fit into two bytes.
* Payload length per frame is capped at 255 bytes to fit the one-byte length field.

## Chunk sizing rules
`QrCapacityCatalog` exposes the maximum number of bytes that can be encoded for every combination of QR version and error-correct
ion level. The effective payload size for a frame is:

```
min(capacity - 6, 255)
```

The subtraction accounts for the 6-byte transport header. If the selected QR configuration cannot fit the header, the sender dis
ables transmission.

## Transmission loop
1. The operator enqueues files via drag-and-drop or the file picker. `QrChunkBuilder` splits every file into slices up to the configured chunk size (capped at 255 bytes) and creates both metadata and data packets.
2. When the Start button is pressed, the sender iterates over the queue. For each file it emits the metadata frames once, followe
d by the data frames in offset order. The UI highlights the current file and shows progress in chunks.
3. Once the data stream reaches the declared total length, the file is marked as completed and the sender advances to the next pe
nding file. If repeat mode is enabled, finished files reset and the loop starts over.
4. Frame timing is controlled by the configured frame duration. The sender waits for the specified number of milliseconds between
 frames while it remains running. Pausing the transmission stops frame generation without clearing progress.
5. Restart clears chunk indices for every queued file, resets the current pointer, and removes the QR preview so the operator can
 start a fresh cycle.

Receivers should assemble frames per file slot, merging metadata and data streams separately. Missing or corrupt data blocks can 
be detected by comparing the reconstructed file against the advertised block checksums. The metadata flag allows receivers to han
dle descriptive information and payload bytes independently.
