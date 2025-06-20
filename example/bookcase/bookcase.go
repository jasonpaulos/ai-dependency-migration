package bookcase

import (
	"encoding/json"

	"github.com/bits-and-blooms/bloom/v3"
)

// Bookcase is a data structure used to store and remember book titles.
// Internally it uses a Bloom filter to efficiently check for the presence of book titles.
type Bookcase struct {
	filter *bloom.BloomFilter
}

// New creates a new Bookcase with a specified desired capacity and false positive rate.
func New(desiredCapacity int, desiredFalsePositiveRate float64) *Bookcase {
	return &Bookcase{
		filter: bloom.NewWithEstimates(uint(desiredCapacity), desiredFalsePositiveRate),
	}
}

// Size return the number of bits in the filter.
func (b *Bookcase) Size() int {
	return int(b.filter.Cap())
}

// ApproximateCount returns the approximate number of unique book titles stored in the Bookcase.
func (b *Bookcase) ApproximateCount() int {
	return int(b.filter.ApproximatedSize())
}

// AddBook adds a book title to the Bookcase.
func (b *Bookcase) AddBook(title string) {
	b.filter.AddString(title)
}

// MightHaveBook checks if a book title might be in the Bookcase.
// It returns true if the title might be present, and false if it is definitely not present.
func (b *Bookcase) MightHaveBook(title string) bool {
	return b.filter.TestString(title)
}

// ToJson serializes the Bookcase to a JSON string.
func (b *Bookcase) ToJson() (string, error) {
	bytes, err := json.Marshal(b.filter)
	if err != nil {
		return "", err
	}
	return string(bytes), nil
}

// FromJson deserializes a JSON string into a Bookcase.
func FromJson(jsonStr string) (*Bookcase, error) {
	var filter bloom.BloomFilter
	if err := json.Unmarshal([]byte(jsonStr), &filter); err != nil {
		return nil, err
	}
	return &Bookcase{filter: &filter}, nil
}
