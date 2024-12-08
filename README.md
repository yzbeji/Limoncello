# Limoncello

Limoncello is a task management and collaboration tool, inspired by Trello, built with the power of **ASP.NET** on the backend and **Bootstrap** for a responsive, sleek frontend. This application allows teams and individuals to organize projects, tasks, and workflows visually using boards, lists, and cards—making it easy to track progress and collaborate efficiently.

## Features

- **Boards & Lists**: Create boards to represent projects and lists to categorize tasks within each board.
- **Task Cards**: Add, edit, and move task cards between lists to reflect the progress of your work.
- **Drag & Drop Interface**: Intuitive drag-and-drop functionality for task management.
- **Responsive Design**: Built using Bootstrap to ensure a seamless experience across all device sizes (desktop, tablet, mobile).
- **User-Friendly Interface**: Simple and modern UI for easy navigation and task tracking.
- **Real-time Updates**: Tasks and boards update instantly, ensuring that all users stay on the same page.
- **Task Assignments**: Assign tasks to different team members and monitor progress.
- **Due Dates and Labels**: Add deadlines and labels to categorize tasks and set priorities.

## Technology Stack

- **Backend**: ASP.NET (C#) - providing a robust, scalable, and secure framework for managing users, tasks, and boards.
- **Frontend**: Bootstrap - a responsive framework for clean, user-friendly UI design.
- **Database**: SQL Server (or any database of your choice) for managing persistent data.
- **Authentication**: ASP.NET Identity for secure user authentication and management.

## Installation & Setup

1. **Clone the repository**:
   ```bash
   git clone https://github.com/yourusername/limoncello.git
   ```

2. **Navigate to the project directory**:
   ```bash
   cd limoncello
   ```

3. **Restore the .NET dependencies**:
   ```bash
   dotnet restore
   ```

4. **Update the database connection**:
   - Go to `appsettings.json` and configure your connection string for your SQL Server instance.

5. **Run the application**:
   ```bash
   dotnet run
   ```

6. **Access the app**:
   - Navigate to `http://localhost:5000` (or your configured port) in your browser.

## Contributing

Contributions are welcome! If you'd like to contribute to Limoncello, please follow these steps:

1. Fork the repository.
2. Create a new branch for your feature or bug fix (`git checkout -b feature-name`).
3. Commit your changes (`git commit -m "Add feature/bug fix"`).
4. Push to the branch (`git push origin feature-name`).
5. Create a Pull Request, and we'll review your changes.
