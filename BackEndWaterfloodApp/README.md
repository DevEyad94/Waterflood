# Grave Management System - Deployment Guide

## Table of Contents

- [Prerequisites](#prerequisites)
- [Server Setup](#server-setup)
- [Database Setup](#database-setup)
- [Application Deployment](#application-deployment)
- [SSL Configuration](#ssl-configuration)
- [Service Configuration](#service-configuration)
- [Nginx Configuration](#nginx-configuration)
- [Directory Structure](#directory-structure)
- [Common Issues](#common-issues)
- [Maintenance](#maintenance)

## Prerequisites

- Linux server (Ubuntu/Debian)
- .NET 9.0 SDK
- PostgreSQL
- Nginx
- SSL certificate
- Domain name pointing to your server

## Server Setup

1. Update server packages:

```bash
sudo apt update && sudo apt upgrade -y
```

2. Install required packages:

```bash
sudo apt install -y nginx postgresql certbot python3-certbot-nginx
```

3. Install .NET 9.0 Runtime:

```bash
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt update && sudo apt install -y aspnetcore-runtime-9.0
```

## Database Setup

1. Configure PostgreSQL:

```bash
sudo -u postgres psql
```

2. Create database and user:

```sql
CREATE DATABASE GraveDB;
CREATE USER GraveUser WITH PASSWORD 'your-strong-password';
GRANT ALL PRIVILEGES ON DATABASE GraveDB TO GraveUser;
\q
```

## Application Deployment

1. Create deployment script (deploy.sh) on your local machine:

```bash
#!/bin/bash

# Build the application
dotnet publish -c Release -r linux-x64 --self-contained true

# Create required directories
ssh root@srv652462.hstgr.cloud 'mkdir -p /var/www/grave/images'

# Sync files to server
rsync -avz --delete -e ssh ./bin/Release/net9.0/linux-x64/publish/ root@srv652462.hstgr.cloud:/var/www/grave/

# SSH into server and restart application
ssh root@srv652462.hstgr.cloud << 'ENDSSH'
    sudo chown -R www-data:www-data /var/www/grave
    sudo chmod -R 755 /var/www/grave
    sudo systemctl restart grave
    echo "Deployment completed!"
ENDSSH
```

2. Make script executable:

```bash
chmod +x deploy.sh
```

## Service Configuration

1. Create systemd service file:

```bash
sudo nano /etc/systemd/system/grave.service
```

2. Add service configuration:

```ini
[Unit]
Description=Grave Application
After=network.target

[Service]
WorkingDirectory=/var/www/grave
ExecStart=/var/www/grave/project
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=grave
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000
Environment=ConnectionStrings__DefaultConnection="Server=147.93.62.36;Port=5432;Database=GraveDB;User Id=GraveUser;Password=your-password;SSL Mode=Require;Trust Server Certificate=true;"

[Install]
WantedBy=multi-user.target
```

3. Enable and start service:

```bash
sudo systemctl enable grave
sudo systemctl start grave
```

## Nginx Configuration

1. Create Nginx configuration:

```bash
sudo nano /etc/nginx/sites-available/grave
```

2. Add configuration:

```nginx
# HTTP - redirect all requests to HTTPS
server {
    listen 80;
    listen [::]:80;
    server_name srv652462.hstgr.cloud;

    # Required for LE certificate renewal
    location /.well-known/acme-challenge/ {
        allow all;
        root /var/www/certbot;
    }

    # Redirect all HTTP traffic to HTTPS
    location / {
        return 301 https://$host$request_uri;
    }
}

# HTTPS configuration
server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name srv652462.hstgr.cloud;

    # SSL Configuration
    ssl_certificate /etc/letsencrypt/live/srv652462.hstgr.cloud/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/srv652462.hstgr.cloud/privkey.pem;

    # SSL optimization
    ssl_session_timeout 1d;
    ssl_session_cache shared:SSL:50m;
    ssl_session_tickets off;
    ssl_protocols TLSv1.2 TLSv1.3;

    # Root directory and index files
    root /var/www/grave/wwwroot/browser;
    index index.html;

    # API proxy configuration
    location /api/ {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;

        # CORS headers
        add_header 'Access-Control-Allow-Origin' '*' always;
        add_header 'Access-Control-Allow-Methods' 'GET, POST, OPTIONS, PUT, DELETE' always;
        add_header 'Access-Control-Allow-Headers' 'DNT,User-Agent,X-Requested-With,If-Modified-Since,Cache-Control,Content-Type,Range,Authorization' alw>
        add_header 'Access-Control-Expose-Headers' 'Content-Length,Content-Range' always;
    }

    # Static file handling with SPA support
    location / {
        try_files $uri $uri/ /index.html;
        expires 30d;
        add_header Cache-Control "public, no-transform";
    }

    # Security headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header Referrer-Policy "no-referrer-when-downgrade" always;
    add_header Content-Security-Policy "default-src 'self' http: https: data: blob: 'unsafe-inline'" always;

    # Enable gzip compression
    gzip on;
    gzip_vary on;
    gzip_min_length 10240;
    gzip_proxied expired no-cache no-store private auth;
    gzip_types text/plain text/css text/xml text/javascript application/x-javascript application/xml;
    gzip_disable "MSIE [1-6]\.";
}
```

3. Enable site and restart Nginx:

```bash
sudo ln -s /etc/nginx/sites-available/grave /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

## SSL Configuration

1. Obtain SSL certificate:

```bash
sudo certbot --nginx -d srv652462.hstgr.cloud
```

2. Auto-renewal verification:

```bash
sudo systemctl status certbot.timer
```

## Directory Structure

```
/var/www/grave/
├── wwwroot/          # Static files and frontend
│   ├── browser/      # Angular application
│   └── images/       # Uploaded images
├── project           # Application executable
└── appsettings.json  # Configuration file
```

## Common Issues

1. **502 Bad Gateway**

   - Check application status: `sudo systemctl status grave`
   - Check logs: `sudo journalctl -u grave -n 100`

2. **Permission Issues**

   ```bash
   sudo chown -R www-data:www-data /var/www/grave
   sudo chmod -R 755 /var/www/grave
   ```

3. **Database Connection Issues**

   - Verify connection string in grave.service
   - Check PostgreSQL status: `sudo systemctl status postgresql`

4. **SSL Certificate Issues**

   - If you hit rate limits: Wait for one hour before trying again
   - Ensure Nginx is configured properly for Let's Encrypt validation

5. **Cloudinary Dependency Issues**
   If you need to remove Cloudinary:
   - Comment out Cloudinary service registration in Program.cs
   - Remove IPhotoAccessor from GraveController constructor
   - Remove ImageUploadCloud method from GraveController

## Maintenance

1. View application logs:

```bash
sudo journalctl -u grave -f
```

2. Restart application:

```bash
sudo systemctl restart grave
```

3. Update SSL certificate:

```bash
sudo certbot renew
```

4. Backup database:

```bash
pg_dump -U GraveUser GraveDB | gzip > backup-$(date +%Y-%m-%d).sql.gz
```

5. Monitor disk space:

```bash
df -h
```

6. Clean up old logs:

```bash
sudo journalctl --vacuum-time=7d
```

## Security Recommendations

1. Enable firewall:

```bash
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw allow 22/tcp
sudo ufw enable
```

2. Regular updates:

```bash
sudo apt update && sudo apt upgrade -y
```

3. Monitor auth logs:

```bash
sudo tail -f /var/log/auth.log
```

---

For any issues or questions, please contact the system administrator.

# Deployment Instructions

This document outlines how to deploy the application to the server.

## Prerequisites

- .NET SDK installed on your local machine
- SSH key pair for authentication (`id_ed25519`)
- Access to the target server (srv652462.hstgr.cloud)

## Deployment Steps

### 1. Configure SSH for Passwordless Authentication

To avoid repeatedly entering your SSH key passphrase, set up your SSH config:

```bash
# Create or edit your SSH config file
mkdir -p ~/.ssh
touch ~/.ssh/config
chmod 600 ~/.ssh/config

# Add the following to your SSH config
echo "Host srv652462.hstgr.cloud
    HostName srv652462.hstgr.cloud
    User root
    IdentityFile ~/.ssh/id_ed25519
    IdentitiesOnly yes
    AddKeysToAgent yes" >> ~/.ssh/config
```

### 2. Add Your SSH Key to the SSH Agent

This stores your key in memory for the session:

```bash
# For macOS:
ssh-add -K ~/.ssh/id_ed25519 || ssh-add --apple-use-keychain ~/.ssh/id_ed25519 || ssh-add ~/.ssh/id_ed25519

# For Linux:
eval "$(ssh-agent -s)"
ssh-add ~/.ssh/id_ed25519
```

### 3. Make the Deployment Script Executable

```bash
chmod +x deploy.sh
```

### 4. Run the Deployment Script

```bash
./deploy.sh
```

## Troubleshooting

- If you still get passphrase prompts, ensure your SSH agent is running with: `ssh-add -l`
- For persistent SSH agent across terminal sessions, add to your `~/.bashrc` or `~/.zshrc`:
  ```bash
  if [ -z "$SSH_AUTH_SOCK" ] ; then
      eval "$(ssh-agent -s)"
      ssh-add
  fi
  ```

## What the Deployment Script Does

1. Builds the .NET application for Linux deployment
2. Creates necessary directories on the server
3. Syncs the built application to the server
4. Installs required dependencies for image processing
5. Configures system services for the application
6. Sets appropriate file permissions
