# DbProvisioner

DbProvisioner is a lightweight .NET code-first utility designed to run as an ephemeral container within your Docker Compose stack. Its primary goal is to automatically configure database users, passwords, and schemas every time you deploy or restart your stack. It's meant to be used on those scenarios that you make use of a single database for multiple services.

It ensures your database access controls are always in sync with your environment configuration, handling the creation of new users and the cleanup of obsolete ones.

**DbProvisioner is currently considered feature complete.** It fulfills its primary purpose of synchronizing database users and schemas via environment variables within a containerized environment. But we are open for PRs with added features, please check the `CONTRIBUTION.md` for more details.

## 🚀 How it Works

When the container starts, it performs the following logic:

1. **Connects** to the target database server (MySQL/MariaDB) using administrative credentials.

1. **Scans Environment Variables** to identify desired users and databases (based on variables matching the pattern `DB_{NAME}_USERNAME`).

1. **Syncs State:**
    * **Creates** users/databases that do not exist.
    * **Updates** passwords for existing users (ensuring credentials are up to date).
    * **Grants** `ALL PRIVILEGES` to the user for their specific database.
    * **Removes** users found in the database but not in the environment variables (excluding system users like `root`).

## 🐳 Docker Compose Integration

This is the recommended way to use DbProvisioner. Add it to your `docker-compose.yml` alongside your database container.

*Note:* The provisioner needs to wait for the database to be healthy before running.

```YAML
services:
  # Your Database Service
  db:
    image: mariadb:10.6 # or mysql:8
    environment:
      MYSQL_ROOT_PASSWORD: root_secret_password
    healthcheck:
      test: ["CMD", "healthcheck.sh", "--connect", "--innodb_initialized"]
      interval: 10s
      timeout: 5s
      retries: 5

  # The Provisioner Service
  db-provisioner:
    build: .
    depends_on:
      db:
        condition: service_healthy
    environment:
      DB_HOST: db
      DB_USER: root
      DB_PASSWORD: root_secret_password
      
      # --- Users to Provision ---
      # Pattern: DB_{NAME}_USERNAME & DB_{NAME}_PASSWORD
      
      # Creates database "APP_ONE" and user "app_user"
      DB_APP_ONE_USERNAME: app_user
      DB_APP_ONE_PASSWORD: secure_pass_123
      
      # Creates database "ANALYTICS" and user "data_svc"
      DB_ANALYTICS_USERNAME: data_svc
      DB_ANALYTICS_PASSWORD: another_secure_pass
```

## ⚙️ Configuration

The application is configured entirely via Environment Variables.

### Connection Settings

These variables tell the provisioner how to connect to the server to perform administrative tasks.
| Variable	| Description |	Default |
| --- | --- | --- |
|`DB_HOST` |	The hostname of the database server (e.g., `db` or `localhost`). | **Required** |
|`DB_USER` |	Admin username (usually `root`).	| **Required** |
|`DB_PASSWORD` |	Admin password.	| **Required** |
|`DB_NAME` |	The initial database to connect to.	| `mysql` |


### User & Database Definition

The provisioner dynamically parses environment variables to find users to create. It looks for keys starting with `DB_` and ending with `_USERNAME`.

Format:

* **Username:** `DB_{DB_NAME}_USERNAME`
* **Password:** `DB_{DB_NAME}_PASSWORD`

*Example:* Setting `DB_MYSHOP_USERNAME=shop_admin` will:
1. Create a database named `MYSHOP`.
1. Create a user named `shop_admin`.
1. Grant `shop_admin` full access to `MYSHOP`.

## 🛡️ Safety & Cleanup

- System Users: The provisioner contains a whitelist of system users (e.g., `root`, `mysql.sys`, `healthcheck`) that it will never delete, even if they are not in your environment variables.
- Stale Users: Any non-system user found in the database that is not defined in your current environment variables will be dropped. This ensures your database doesn't accumulate "zombie" accounts from old configurations.

## 🛠️ Built With

- .NET 10
- MySqlConnector
- Docker (Alpine Linux)

