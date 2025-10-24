# QR Transfer Protocol

## QR payload format
Each QR frame embeds a compact JSON envelope serialized by `QrTransferSenderTab`. The envelope uses the following fields:

| Field | Type | Description |
| --- | --- | --- |
| `t` | `string` | Frame type. The only supported value today is `"chunk"`. |
| `fid` | `Guid` | Stable identifier assigned to the source file. It persists across retransmissions so receivers can resume. |
| `name` | `string` | Original file name. Receivers reuse it when offering the reconstructed download. |
| `fs` | `long` | Original file length in bytes. |
| `cs` | `int` | Declared chunk size in bytes. All chunks except the last should match this size. |
| `ecc` | `string` | QR error-correction level used by the sender (`L`, `M`, `Q`, or `H`). |
| `ci` | `int` | Zero-based index of the current chunk in the file stream. |
| `tc` | `int` | Total number of chunks for the file with the current chunk size. |
| `p` | `string` | Base64-encoded data slice constrained by the QR version and error-correction level. |
| `crc` | `uint` | CRC32 checksum of the decoded payload to validate chunk integrity. |
| `fcrc` | `uint` | CRC32 checksum of the entire file calculated before splitting it into chunks. |

The JSON string is UTF-8 encoded and then wrapped into a single QR segment using `Net.Codecrete.QrCodeGenerator`. Receivers decode the Base64 payload, verify the chunk CRC, and copy the bytes into a fixed 64 KB in-memory buffer. Once every chunk arrives, the receiver recomputes the file-level CRC (`fcrc`) to confirm integrity before exposing a download link.

## Chunk sizing rules
`QrCapacityCatalog` exposes the maximum payload capacity (in bytes) for each QR version and error-correction level. The sender validates the user-selected chunk size against the catalog before the transmission starts or when settings change. If the chunk size exceeds the capacity, the sender blocks transmission until the value falls below the allowed limit.

## Transmission loop
1. The operator enqueues files via drag-and-drop or the file picker. `QrChunkBuilder` splits every file into byte chunks with the configured size and tracks metadata in `QrChunkPacket` instances.
2. When the Start button is pressed, the sender iterates over the queue in order. For each file it emits QR frames sequentially, increasing `ChunkIndex` after every frame. The UI highlights the current file and shows progress in chunks.
3. Once a file reaches `TotalChunks`, it is marked as completed and the sender advances to the next pending file. If repeat mode is enabled, finished files are reset and the loop restarts from the beginning.
4. Frame timing is controlled by the configured frame rate. The sender waits `1000 / FrameRate` milliseconds between frames while it remains in the running state. Pausing the transmission stops frame generation without clearing progress.
5. Restart clears chunk indices for every queued file, resets the current pointer, and removes the QR preview so the operator can start a fresh cycle.

Receivers should watch the QR stream, parse frames, and assemble files in order of their `fid`. Missing or corrupt chunks can be detected through gaps in `ci` or CRC mismatches and requested again during repeated broadcasts. Failed file-level checksum validations trigger another pass while keeping metadata and partially received bytes in memory.

## Receiver implementation notes

The WebAssembly receiver performs QR decoding in .NET via [`ZXing.Net`](https://github.com/micjahn/ZXing.Net). `BrowserMediaCapture` configures screen or camera capture, blits frames into an off-screen canvas, and exposes RGBA buffers directly to managed code without additional JavaScript helpers. The managed decoder extracts QR content even when the tab is backgrounded, then forwards decoded payloads to `QrChunkAssembler` for CRC validation and file reconstruction.
