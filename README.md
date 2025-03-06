EV Charging Session Management System

Overview
This project implements a real-time EV charging session management system using React (frontend) and .NET (backend) that communicates via a private MQTT broker.
The system tracks charging sessions, stores session data in MongoDB, and updates the charging status in real-time. 
The entire solution is containerized using Docker and can be deployed to any cloud platform.

System Architecture
The system consists of the following components:

Frontend: React application with TypeScript that provides a user interface for starting and stopping charging sessions and displays real-time charging status.

Backend: .NET 7+ C# application that processes MQTT messages and manages charging sessions.

MQTT Broker: Self-hosted MQTT broker (Eclipse Mosquitto) that facilitates communication between frontend and backend.
MongoDB: Database for storing charging session data.

Table of Contents

    1.Features
    2.Prerequisites
    3.Installation
    4.Running the Application
    5.API Documentation
    6.Database Schema
    7. MQTT TOPICS 
Features

* Start and stop EV charging sessions
* Real-time tracking of charging status and energy consumption
* Persistent storage of session data in MongoDB
* REST API endpoints for manual control and debugging
* Fully containerized deployment with Docker
* Cloud-ready deployment configuration


Prerequisites
Before running EVCSM, ensure you have the following installed:
* Docker
* .NET SDK 
* Node.js (if you plan to run the frontend separately)


Installation
Clone the project to your local machine:

git clone https://github.com/aulphar/EVCSM.git
cd EVCSM

Build and Run with Docker Compose
Build and start the containers with:

docker-compose up --build

Running Locally (Alternative)
If you prefer to run the services without Docker:
* Backend: Navigate to the EVCSMBackend directory and run:
dotnet run

* Frontend: Navigate to the EVCSMBackend directory and run:
npm install
npm start


Running the Application
Once running, you can access:
* Backend API: http://localhost:5274/api/ChargingSessionAPI
* Frontend Application: http://localhost:3000 
You can use tools like Postman to interact with the API endpoints or navigate via your browser to test the frontend interface. You can also use this  http://localhost:5274/swagger/index.html to access the API using swagger


API Endpoints
Charging Session Management
* Start Charging Session
    * Initiates a new charging session
    * ChargingSessionAPI/start
    * Response: 200 OK with session details
* Stop Charging Session    
    * ChargingSessionAPI/stop
    * Terminates an active charging session
    * Response: 200 OK with updated session details
* Get Charging Status
    * ChargingSessionAPI/status
    * Retrieves the latest session data
    * Response: 200 OK with session details


MongoDB Schema
Charging Session

{
  "_id": "ObjectId('65b7e2e5e3b75c0012345678')", //Generated autmatically
  "sessionId": "UUID-12345",
  "startTime": "2025-02-25T12:00:00Z", // Generated automatically
  "endTime": "2025-02-25T12:30:00Z",  // Only present for completed sessions
  "status": "Charging",               // "Charging" or "Stopped"
  "energyConsumed": 15.2              // kWh consumed during the session
}

MQTT Topics
* Charging Updates(“Charging”/“Stopped”): “charging/updates”
* Session Updates(Session details eg Energy consumed): “session/updates”

