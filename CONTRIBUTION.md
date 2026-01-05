# Contributing to DbProvisioner

First off, thank you for considering contributing to this project!

## Project Status: Feature Complete

**DbProvisioner is currently considered feature complete.** It fulfills its primary purpose of synchronizing database users and schemas via environment variables within a containerized environment.
We are not actively looking to add large new architectural features or change the core logic of the application, but we will consider any proposal. 
We aim to keep this tool small, focused, and "do one thing well."

## What We Welcome

Even though the project is feature complete, we are always open to improvements! We welcome pull requests for:

* **Bug Fixes:** If you find a bug, please feel free to fix it.
* **Security Updates:** Updates to dependencies or logic to improve security.
* **Performance:** Small optimizations that make the sync process faster or use fewer resources.
* **Documentation:** Clarifications, fixing typos, or adding better examples to the README.
* **Compatibility:** Improving support for different versions of MySQL or MariaDB.

## How to Contribute

1. **Fork the repository** and create your branch from main.
1. **Make your changes.** If you are fixing a bug, try to ensure your code is clean and well-commented.
1. **Test your changes.** Ensure the provisioner still builds correctly via the Dockerfile and performs as expected in a local Docker Compose setup.
1. **Submit a Pull Request.** Provide a clear description of what your changes do and why they are beneficial.

## Code Style

* Follow standard C# coding conventions.
* Keep the code simple. This is a utility tool; readability and maintainability are our priorities.
* Use descriptive commit messages.

## Questions?

If you have an idea for a major change, please open an Issue first to discuss it with the maintainers before putting in the work for a Pull Request.

Thank you for helping make DbProvisioner better!