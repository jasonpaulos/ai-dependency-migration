package bookcase

import (
	"testing"

	"github.com/stretchr/testify/require"
)

func TestBookcaseAdd(t *testing.T) {
	bookcase := New(1000, 0.01)
	bookcase.AddBook("The Great Gatsby")

	require.True(t, bookcase.MightHaveBook("The Great Gatsby"))
	require.False(t, bookcase.MightHaveBook("3000 Leagues Under the Sea"))
}
