package bookcase

import (
	"encoding/json"
	"errors"

	cuckoo "github.com/seiflotfy/cuckoofilter"
)

// Bookcase is a data structure used to store and remember book titles.
// Internally it uses a Cuckoo filter to efficiently check for the presence of book titles.
type Bookcase struct {
	filter *cuckoo.Filter
}

// New creates a new Bookcase with a specified desired capacity and false positive rate.
// Note: cuckoofilter does not use a false positive rate, so we ignore it and only use capacity.
func New(desiredCapacity int, _ float64) *Bookcase {
	return &Bookcase{
		filter: cuckoo.NewFilter(uint(desiredCapacity)),
	}
}

// Size returns the approximate filter's bucket count (not bits, but close).
func (b *Bookcase) Size() int {
	// There is no direct "size in bits" because Cuckoo filter is hash+fingerprint based. Bucket count is a rough equivalent.
	return int(b.filter.Count())
}

// ApproximateCount returns the approximate number of unique book titles stored in the Bookcase.
func (b *Bookcase) ApproximateCount() int {
	return int(b.filter.Count())
}

// AddBook adds a book title to the Bookcase.
func (b *Bookcase) AddBook(title string) {
	b.filter.Insert([]byte(title))
}

// MightHaveBook checks if a book title might be in the Bookcase.
// It returns true if the title might be present, and false if it is definitely not present.
func (b *Bookcase) MightHaveBook(title string) bool {
	return b.filter.Lookup([]byte(title))
}

// ToJson serializes the Bookcase to a JSON string.
// Note: Encodes the underlying filter as bytes then base64 json string.
func (b *Bookcase) ToJson() (string, error) {
	encoded := b.filter.Encode()
	j, err := json.Marshal(encoded)
	if err != nil {
		return "", err
	}
	return string(j), nil
}

// FromJson deserializes a JSON string into a Bookcase.
func FromJson(jsonStr string) (*Bookcase, error) {
	var encoded []byte
	if err := json.Unmarshal([]byte(jsonStr), &encoded); err != nil {
		return nil, err
	}
	filter, err := cuckoo.Decode(encoded)
	if err != nil {
		return nil, err
	}
	if filter == nil {
		return nil, errors.New("failed to decode cuckoo filter")
	}
	return &Bookcase{filter: filter}, nil
}
