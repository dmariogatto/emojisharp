# EmojiSharp

Just a random project to play with Azure Table Storage and Functions.

## EmojiSharp.Loader

Downloads the [Unicode Emoji List](https://unicode.org/emoji/charts/emoji-list.html) and converts it into JSON format. Then connects to an Azure Table Store and loads in all the emoji goodness.

## EmojiSharp.Functions

A collection of Azure Functions that provides an API for accessing the Table Storage.