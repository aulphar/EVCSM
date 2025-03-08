EV Charging Session Management System
# EV Charging Session Management System
## Overview
This project implements a real-time EV charging session management system using React
(frontend) and .NET (backend) that communicates via a private MQTT broker.
The system tracks charging sessions, stores session data in MongoDB, and updates the charging
status in real-time.
The entire solution is containerized using Docker and can be deployed to any cloud platform.
## System Architecture
The system consists of the following components:
- **Frontend:** React application with TypeScript that provides a user interface for starting and
stopping charging sessions and displays real-time charging status.
- **Backend:** .NET 7+ C# application that processes MQTT messages and manages charging
sessions.
- **MQTT Broker:** Self-hosted MQTT broker (Eclipse Mosquitto) that facilitates communication
between frontend and backend.
- **MongoDB:** Database for storing charging session data.
## Table of Contents
1. Features
2. Prerequisites
3. Installation
4. Running the Application
5. API Documentation
6. Database Schema
7. MQTT Topics
## Features
* Start and stop EV charging sessions
* Real-time tracking of charging status and energy consumption
* Persistent storage of session data in MongoDB
* REST API endpoints for manual control and debugging
* Fully containerized deployment with Docker
* Cloud-ready deployment configuration
## Prerequisites
Before running EVCSM, ensure you have the following installed:
- Docker
- .NET SDK
- Node.js (if you plan to run the frontend separately)
## Installation
Clone the project to your local machine:
```bash
git clone https://github.com/aulphar/EVCSM.git
cd EVCSM
```
### Build and Run with Docker Compose
Build and start the containers with:
```bash
docker-compose up --build
```
### Running Locally (Alternative)
If you prefer to run the services without Docker:
- **Backend:**
Navigate to the EVCSMBackend directory and run:
```bash
dotnet run
```
- **Frontend:**
Navigate to the EVCSMFrontend directory and run:
```bash
npm install
npm start
```
## Running the Application
Once running, you can access:
- **Backend API:**
[http://localhost:5274/api/ChargingSessionAPI](http://localhost:5274/api/ChargingSessionAPI)
- **Frontend Application:** [http://localhost:3000](http://localhost:3000)
You can use tools like Postman to interact with the API endpoints or navigate via your browser to
test the frontend interface.
You can also use this
[http://localhost:5274/swagger/index.html](http://localhost:5274/swagger/index.html) to access the
API using swagger.
## FrontEnd
### Base URL
```
http://ec2-51-20-48-234.eu-north-1.compute.amazonaws.com:8080
```
This link takes you directly to the project website where you can test out the project functionalities
like starting and stopping a charging session. A status bar that displays whether a charging session
is "Charging" or "Stopped" and an Energy Consumed counter.
## API Documentation
### Base URL
```
http://ec2-51-20-48-234.eu-north-1.compute.amazonaws.com:8080/api/ChargingSessionAPI
```
### 1. Get Session Status
Retrieves the latest or currently active charging session.
- **Endpoint:** `/status`
- **Method:** GET
- **Request:** None
**Example Response:**
```json
{
"sessionId": "cs-1234567890",
"status": "ACTIVE",
"startTime": "2025-03-08T10:15:30Z",
"endTime": null,
"energyConsumed": 5.67,
"durationMinutes": 45
}
```
**Status Codes:**
- `200 OK` - Request successful
- `404 Not Found` - No session found
- `500 Internal Server Error` - Server error
### 2. Start Charging Session
Initiates a new charging session.
- **Endpoint:** `/start`
- **Method:** POST
- **Request:** JSON
```json
{
"userId": "string",
"vehicleId": "string",
"chargingPointId": "string",
"maxPower": 11.0,
"targetCharge": 80
}
```
**Example Response:**
```json
{
"sessionId": "cs-1234567890",
"status": "STARTED",
"startTime": "2025-03-08T10:15:30Z",
"message": "Charging session started successfully",
"estimatedDuration": 120
}
```
**Status Codes:**
- `201 Created` - Session created successfully
- `400 Bad Request` - Invalid request parameters
- `409 Conflict` - Active session already exists
- `500 Internal Server Error` - Server error
### 3. Stop Charging Session
Terminates an active charging session.
- **Endpoint:** `/stop`
- **Method:** POST
- **Request:** JSON
```json
{
"sessionId": "cs-1234567890"
}
```
**Example Response:**
```json
{
"sessionId": "cs-1234567890",
"status": "STOPPED",
"startTime": "2025-03-08T10:15:30Z",
"endTime": "2025-03-08T11:45:30Z",
"energyConsumed": 15.5,
"durationMinutes": 90,
"cost": 12.75,
"message": "Charging session stopped successfully"
}
```
**Status Codes:**
- `200 OK` - Session stopped successfully
- `400 Bad Request` - Invalid request parameters
- `404 Not Found` - Session not found
- `500 Internal Server Error` - Server error
### Error Responses
All endpoints may return error responses in the following format:
```json
{
"error": "string",
"message": "string",
"timestamp": "string"
}
```
## MongoDB Schema
### Charging Session
```json
{
"_id": "ObjectId('65b7e2e5e3b75c0012345678')",
"sessionId": "UUID-12345",
"startTime": "2025-02-25T12:00:00Z",
"endTime": "2025-02-25T12:30:00Z",
"status": "Charging",
"energyConsumed": 15.2
}
```
## MQTT Topics
- **Charging Updates:** "charging/updates" - "Charging" or "Stopped"
- **Session Updates:** "session/updates" - Session details (e.g., Energy consumed)
## Authentication
Currently, the API does not require authentication. Future versions may implement authentication
mechanisms.
---
NB: As this is a challenge project some industry standard practices were not implemented eg.
Security, lack of dependency injection when appropriate.
