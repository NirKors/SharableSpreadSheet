# Multi-Threaded Spreadsheet in C#

This project, made while studying for the degree, provides a flexible and efficient spreadsheet implementation that supports concurrent operations using multi-threading techniques.

## Overview

This project aims to provide a multi-threaded spreadsheet implementation in C# that allows users to perform various operations such as setting and getting cell values, searching for specific strings, exchanging rows and columns, and more. The spreadsheet is designed to handle concurrent operations from multiple users efficiently.

## Features

- **Concurrent Operations**: The spreadsheet supports concurrent operations using multi-threading techniques, allowing multiple users to work simultaneously.
  
- **Thread-Safe**: With careful synchronization mechanisms in place, the spreadsheet ensures thread safety to prevent data corruption and maintain consistency.
  
- **Basic Spreadsheet Operations**: Users can perform basic spreadsheet operations such as setting and getting cell values, searching for strings, exchanging rows and columns, adding rows and columns, and more.
  
- **Flexible Design**: The design of the spreadsheet allows for flexibility in terms of adding new features or extending existing functionality.

## Usage

The project provides a `SharableSpreadSheet` class that serves as the core implementation of the multi-threaded spreadsheet. Users can instantiate this class and perform various operations using its methods.

## License

This project is licensed under the [MIT License](LICENSE).
