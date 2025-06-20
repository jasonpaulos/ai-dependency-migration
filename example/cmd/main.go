package main

import (
	"bufio"
	"fmt"
	"os"
	"strings"

	"example.com/example/bookcase"
)

func main() {
	bookcase := bookcase.New(1000, 0.01)

	reader := bufio.NewReader(os.Stdin)

	fmt.Println("Welcome to the Bookcase program! Enter a command below. Valid commands are: 'add', 'check', 'clear', 'json', and 'exit'.")

	for {
		fmt.Print("Enter command: ")
		command, err := reader.ReadString('\n')
		if err != nil {
			fmt.Println("Error reading input:", err)
			os.Exit(1)
		}

		command = strings.TrimSpace(command)
		switch command {
		case "add":
			fmt.Print("Enter book title to add: ")
			title, err := reader.ReadString('\n')
			if err != nil {
				fmt.Println("Error reading input:", err)
				os.Exit(1)
			}
			title = strings.TrimSpace(title)
			if title == "" {
				fmt.Println("Book title cannot be empty. Please try again.")
				continue
			}
			bookcase.AddBook(title)
			fmt.Printf("Added book: \"%s\"\n", title)
			fmt.Printf("Estimated bookcase count: %d\n", bookcase.ApproximateCount())
		case "check":
			fmt.Print("Enter book title to check: ")
			title, err := reader.ReadString('\n')
			if err != nil {
				fmt.Println("Error reading input:", err)
				os.Exit(1)
			}
			title = strings.TrimSpace(title)
			if bookcase.MightHaveBook(title) {
				fmt.Printf("The book \"%s\" might be in the bookcase.\n", title)
			} else {
				fmt.Printf("The book \"%s\" is definitely not in the bookcase.\n", title)
			}
		case "clear":
			bookcase.Clear()
			fmt.Println("Bookcase cleared.")
		case "json":
			jsonStr, err := bookcase.ToJson()
			if err != nil {
				fmt.Println("Error converting bookcase to JSON:", err)
				os.Exit(1)
			}
			fmt.Println("Bookcase JSON representation:")
			fmt.Println(jsonStr)
		case "exit":
			fmt.Println("Exiting the program. Goodbye!")
			os.Exit(0)
		default:
			fmt.Println("Invalid command. Please enter 'add', 'check', 'json', or 'exit'.")
		}
	}
}
