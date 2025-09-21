# Demo Repository
**Demo online:** [https://dimonsmart.github.io/Demo/](https://dimonsmart.github.io/Demo/)

This repository contains a set of interactive Blazor pages that showcase algorithms, developer tools, and experiments I often talk about on my channel. Many of these ideas live in dedicated repositories and NuGet packages; this site aggregates them in a single playground.

## Pages
### StringDiff Demo
Visualizes differences between two pieces of text in real time. It supports both classic character diffs and a "break on words" mode, which makes prose or code reviews easier to follow.

### HashX Research Demo
Allows you to upload a sample file and compare collision statistics for MD5, SHA1, and a few handcrafted hashes. The custom hashes can sometimes outperform cryptographic ones on this benchmark precisely because they skip the avalanche requirement—flipping a bit does not need to flip many output bits—which lets them be tuned for low collisions on specific datasets even if that property would normally limit their range of behaviours.

### Maze Generator Demo
Builds mazes with configurable width, height, density, and wall-shortening parameters. You can slow the visualization to watch the carving process cell by cell.

### Maze Path Finder Demo
Generates a maze and lets you pick start and finish cells to see a wave-propagation search find the shortest route. The visualization can paint both the exploration wave and the reconstructed path step by step.

### Maze Runner Demo
Combines a maze, a simulated robot, and joystick or natural-language controls. You can steer manually or let a local Ollama or OpenAI-powered assistant interpret commands and move the robot while streaming its reasoning log.

### Image Watermark Demo
Adds tiled watermarks to uploaded images entirely in the browser. Rotation, opacity, density, color, and optional resizing let you quickly produce shareable previews without sending files to a server.

### Markdown to Word Demo
Offers a two-pane Markdown editor with on-the-fly preview and tooling for invisible characters. You can highlight, filter, or clean hidden symbols before exporting the rendered document as a Word-compatible download handled purely on the client.

### Traveling Salesman Demo
Explores a genetic algorithm approach to the Traveling Salesman Problem. You can tune population, crossover, mutation, and stopping criteria while watching the algorithm evolve routes on an interactive map.

### Blackjack Demo *(work in progress)*
Simulates a multi-deck blackjack table with dealer and player hands, plus responsive controls. The page is being extended with an analyzer that will compute optimal play tables under different casino rule sets.
