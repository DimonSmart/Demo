# QR Transfer Protocol

## QR payload format
Each QR frame embeds a JSON document serialized by `QrTransferSenderTab`. The document contains the following fields:

| Field | Type | Description |
| --- | --- | --- |
| `FileId` | `Guid` | Stable identifier assigned to the source file. It persists across retransmissions so that receivers can resume. |
| `FileNameHash` | `uint` | CRC32 hash of the original file name, used to verify that metadata matches the expected file. |
| `FileSize` | `long` | Original file length in bytes. |
| `ChunkIndex` | `int` | Zero-based index of the current chunk in the file stream. |
| `TotalChunks` | `int` | Total number of chunks for the file with the current chunk size. |
| `Payload` | `string` | Base64-encoded data slice whose length is constrained by the QR version and error-correction level. |
| `Crc32` | `uint` | CRC32 checksum of the decoded payload to validate chunk integrity. |

The JSON string is UTF-8 encoded and then wrapped into a single QR segment using `Net.Codecrete.QrCodeGenerator`. The resulting SVG is displayed in the sender UI. Receivers must parse the JSON, decode the Base64 payload, validate the CRC32 checksum, and append the bytes to the file stream under reconstruction.

## Chunk sizing rules
`QrCapacityCatalog` exposes the maximum payload capacity (in bytes) for each QR version and error-correction level. The sender validates the user-selected chunk size against the catalog before the transmission starts or when settings change. If the chunk size exceeds the capacity, the sender blocks transmission until the value falls below the allowed limit.

## Transmission loop
1. The operator enqueues files via drag-and-drop or the file picker. `QrChunkBuilder` splits every file into byte chunks with the configured size and tracks metadata in `QrChunkPacket` instances.
2. When the Start button is pressed, the sender iterates over the queue in order. For each file it emits QR frames sequentially, increasing `ChunkIndex` after every frame. The UI highlights the current file and shows progress in chunks.
3. Once a file reaches `TotalChunks`, it is marked as completed and the sender advances to the next pending file. If repeat mode is enabled, finished files are reset and the loop restarts from the beginning.
4. Frame timing is controlled by the configured frame rate. The sender waits `1000 / FrameRate` milliseconds between frames while it remains in the running state. Pausing the transmission stops frame generation without clearing progress.
5. Restart clears chunk indices for every queued file, resets the current pointer, and removes the QR preview so the operator can start a fresh cycle.

Receivers should watch the QR stream, parse frames, and assemble files in order of their `FileId`. Missing or corrupt chunks can be detected through gaps in `ChunkIndex` or CRC mismatches and requested again during repeated broadcasts.
