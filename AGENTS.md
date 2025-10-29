# StarCore design document

## The problem

I lack an external structure that forces me to do the right thing at the right time. I need something that recognizes certain behaviors or responds to certain triggers and performs the right actions intelligently.

## The solution

A framework for creating systems and protocols that help you do the right things at the right time.

### Systems

Systems perform actions intelligently. You can interact with them through a self-defined graphic interface. Systems can connect and exchange data with others, providing the  flexibility to adapt to external variables.

### Protocols

Protocols perform actions in response to triggers. Multiple instances of the same protocol can be opened at once, and you can interact with each of them through a self-defined graphic interface, similar to systems.

## The software

The software provides you with the tools you need to easily create your own systems and protocols:

- An architecture in which a server manages systems and protocols and clients interact with it
- A cross-platform app for clients to interact with their systems and protocols, build with Avalonia UI
    - An interface that allows you to open systems and protocols, to close protocol instances and to open new ones
    - An interface for each system and protocol instance in which they define UI elements for users to interact with them
- A C# framework for creating systems and protocols
    - A UI framework to interact with the client app
    - A framework to interact with clients and perform native actions such as showing notifications or interacting with their screentime API
    - A framework for data persistence between server restarts
    - A framework to interact with AI
    - A framework to interact with your Telos file. A Telos file describes your life, including your problems, your history, your missions and your goals.

## Usage examples

### Focus Protocol

A protocol for focusing on tasks and projects. The protocol allows me to choose a duration and it displays my goal for the entire session.

### Calendar System

A system in which I can define events and persistent time blocks which I can relate to my projects. When a time block starts, a new Focus Protocol is automatically opened.

### Project Manager

A system in which I can register project ideas and the projects that I'm working on. The Project Manager integrates with the Calendar System so that projects can have time blocks on my calendar.

### Task System

A system in which I can define tasks I have to do and give them a do and a due date. I can relate tasks to projects I've defined on the Project Manager. The Focus Protocol allows you to choose a task from the Task System to focus on.

### Capture System

A system for writing down my thoughts. When I add a new entry, an AI decides:
- If the entry is a project idea, the system registers it as an idea on the Project Manager
- If the entry is an event, the system adds it to the Calendar System
- If the entry is a task, the system adds it to the Task System

### Music System

The Music System gets data about my mood (time of the day, focus protocols, routines, events, etc.) and plays ambient music accordingly.

### Other examples

- When I create an exam event on my calendar, a new Exam Protocol opens which adds revision tasks to the Task System
- When I send a print to my 3D printer, a new Printing Protocol opens which reminds me to ventilate the area
- When I'm mindlessly scrolling through YouTube, a protocol opens which blocks it and reminds me to take a break
- A Decision Protocol recommends an option based on my goals defined on my Telos file using AI