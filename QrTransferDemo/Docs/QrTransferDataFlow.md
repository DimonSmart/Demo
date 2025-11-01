# QR Transfer Data Flow Analysis

## Overview

The **QR Transfer** tab generates a sequence of QR frames, each encoding a binary packet of either **metadata** or **data** stream. Reception is resilient to drops, duplicates, and late start: assembling is idempotent and based on offsets and a bitmask of received bytes. Typically, metadata are transmitted first, followed by data; however, the receiver may accept data before metadata and validate them after the metadata are parsed.

Each file is served by its own file slot (indexes 0–15).

## Transmission Frame Format

A frame is formed in `QrTransferSenderTab.SerializePacket` and includes a **5-byte** header followed by the payload.

| Offset | Size | Description                                                                                                       |
| ------ | ---- | ----------------------------------------------------------------------------------------------------------------- |
| 0      | 1    | **Flags**: bit7 = 1 → metadata, 0 → data; bit0–3 — file index `0–15`; bit4–6 — reserved and set to zero.          |
| 1      | 2    | **Total stream length** (metadata or data) in bytes, `ushort` little-endian (`0–65535`).                          |
| 3      | 2    | **Offset** of the payload within the stream, `ushort` little-endian.                                              |
| 5      | *N*  | **Payload** (raw bytes). The length is **not transmitted** and is computed by the receiver as `frame.Length - 5`. |

Chunking is performed in `QrChunkBuilder.SplitIntoPackets`. Chunks have size `chunkSize` (the last one may be shorter). Constraints: `offset ∈ [0, totalLength]`, `offset + chunkLen ≤ totalLength`. The `chunkSize` parameter is chosen according to the QR capacity for the selected error-correction level.

## Metadata Format

Metadata are built in `QrChunkBuilder.BuildMetadata` and contain transfer parameters and **a single** CRC-32 checksum for the entire file.

| Offset  | Size | Description                                                        |
| ------- | ---- | ------------------------------------------------------------------ |
| 0       | 1    | File name length in UTF‑8 = `L`.                                   |
| 1       | *L*  | File name (UTF‑8).                                                 |
| 1 + *L* | 2    | File size `fileSize` in bytes (`0–65535`).                         |
| 3 + *L* | 1    | Chunk size `chunkSize` (bytes).                                    |
| 4 + *L* | 1    | QR error-correction level (ASCII character or `0` if unspecified). |
| 5 + *L* | 4    | **CRC‑32 of the entire file** (polynomial `0xEDB88320`).           |

## Sender Algorithm

1. The user selects files; each is assigned an index (0–15) and a byte buffer.
2. `QrChunkBuilder.BuildPackets` creates queues: a metadata stream, then a data stream. Both streams are split into chunks by `chunkSize`.
3. The transmission loop in `QrTransferSenderTab` forms a frame for each chunk with a 5‑byte header `(flags, totalLength, offset)` and the payload. The QR code is generated via `QrCode.EncodeSegments`.
4. The frame timer (`_frameDuration`) sets a pause between QR frames; if the queue is empty, frames are not generated.
5. For each index, the content (metadata/data) is immutable and the cycle is repeated as needed for robust reception.

## Reception Frame Format

After capturing an image, `QrFrameDecoder.TryDecode` extracts the QR raw bytes. Next, `QrTransferReceiverTab.TryParsePacket` interprets the **5‑byte** header and payload, checking:

* correctness of reserved bits and the file index range;
* initial fixation of the stream `totalLength` upon the first frame;
* no out‑of‑bounds writes: `offset < totalLength`; write is limited to `writeLen = min(payloadLength, totalLength - offset)`; any trailing excess is ignored.

The parser returns a `QrChunkPacket`, which is then passed to the chunk assembler.

## Receiver Algorithm

Assembly is idempotent, tolerates drops, duplicates, and starting not from the first frame.

**Per‑stream structures (metadata/data):**

* `buffer[0..totalLength)`,
* bitmask `received[0..totalLength)` (1 bit per byte),
* `receivedCount` — number of uniquely received bytes,
* for metadata — `contiguousPrefix` — the maximum hole‑free prefix.

**Applying a frame:**

1. If the stream `totalLength` is not yet known — fix it from the frame, allocate `buffer` and `received` of the required length.
2. If `offset ≥ totalLength` — ignore the frame.
3. `payloadLength = frame.Length - 5`, `writeLen = min(payloadLength, totalLength - offset)`; copy `writeLen` bytes, set bits in `received`, update `receivedCount`. Duplicates are harmless.
4. For metadata, update `contiguousPrefix` (while bits are set consecutively starting from zero).

**Metadata readiness:** when `contiguousPrefix == totalLength` — run `TryParseMetadata`. Until then, metadata are not parsed, but data frames may be accepted and laid out according to the bitmask.

**Data readiness and completion:** when `receivedCount == totalLength` — compute CRC‑32 of the entire file and compare with the metadata. If equal — emit `AssembledFile`; otherwise — `InvalidFileChecksum` and wait for correct frames in the next cycle.

**Errors and diagnostics:**

* `InvalidFlags` — reserved bits violated / index out of range;
* `InvalidTotalLength` — mismatch of `totalLength` within one stream;
* `OutOfRangeOffset` — `offset ≥ totalLength`;
* `InvalidMetadata` — failed to parse metadata after full assembly;
* `InvalidFileChecksum` — file CRC‑32 mismatch.

> Excluded: `InvalidChunkLength` — payload length is not part of the protocol; extra bytes are trimmed, missing bytes are supplied by subsequent frames.

## State Snapshot and UI

`FileAssemblySnapshot` reflects assembly progress and state: overall progress (`receivedCount/totalLength`), metadata `contiguousPrefix`, list of yet‑to‑be‑received ranges, CRC errors, etc.

## Transmission vs. Reception Formats

* **Frame header**: unified **5‑byte** format `(flags, totalLength, offset)`; 16‑bit fields are little‑endian. There is no explicit “payload length” field — the receiver uses the actual frame size.
* **Metadata**: structure created by `BuildMetadata` matches `TryParseMetadata` expectations; a **single CRC‑32** is applied to the entire file.
* **Data chunks**: the sender splits streams into chunks of size `chunkSize`; the receiver writes within buffer bounds, duplicates are idempotent, gaps are filled by repeating the cycle.

Thus, the format is resilient to drops, duplicates, and late start. The integrity of the result is checked with a single CRC‑32 after full assembly.
