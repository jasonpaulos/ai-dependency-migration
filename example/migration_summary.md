# Migration Summary: Bloom Filter → Cuckoo Filter

This project previously used [`github.com/bits-and-blooms/bloom/v3`](https://pkg.go.dev/github.com/bits-and-blooms/bloom/v3) for its filter functionality. It now uses [`github.com/seiflotfy/cuckoofilter`](https://pkg.go.dev/github.com/seiflotfy/cuckoofilter). Below are the main changes and caveats involved.

## Code Changes

- **Replaced all references** to BloomFilter in `bookcase.go` with Cuckoo Filter equivalents.
  - Construction: Only `desiredCapacity` is used by CuckooFilter. `desiredFalsePositiveRate` is ignored, as Cuckoo filters do not support this parameter directly.
  - Methods updated:
    - Creation with `NewFilter(uint(desiredCapacity))`.
    - Addition: `.AddBook()` now uses `Insert([]byte(title))`.
    - Checking: `.MightHaveBook()` now uses `Lookup([]byte(title))`.
    - Count approximations: Uses `.Count()`.
    - Serialization: Instead of direct JSON for the struct, the filter is encoded with `.Encode()` and stored as a JSON array of bytes. Deserialization decodes with `cuckoo.Decode()`.
  - The rest of the project (including tests and main program) remained compatible due to the abstraction layer.

## Limitations and Differences
- **False Positive Rate**: You can no longer configure the false positive rate directly; it is determined by filter size and internal algorithm.
- **Serialization**: Cuckoo filter is serialized as bytes (not as a direct struct with JSON fields, as with the bloom filter), but the ToJson/FromJson API remains compatible at the application level.
- **Function signatures** remained the same, although internal semantics changed.

## Tests
- All existing tests in `bookcase_test.go` passed without modification.

---
If you have custom code that uses underlying `bloom.BloomFilter` features (e.g., merging, advanced hash controls), manual migration for that code will be needed.