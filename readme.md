# CMS - Doctor & Appointment Booking System

Welcome to the Clinic Management System (CMS). This guide outlines the setup instructions for creating doctors and managing the appointment booking process.

## 🚀 Setup Instructions

### 1. Database Configuration
Ensure your database connection is properly configured in appsetting.json /appsettings.Development.json.

### 2. run database script

Database/ClinicAppointments.sql

### 3. API Endpoints Overview

#### 🩺 Doctor Management
*   **List of Doctors:** `GET /api/v2/Doctor`
*   **List of Appointment:** `GET /api/v2/Appointment`



## 🛠️ Typical Workflow
1.  **Admin** creates a new Doctor profile specifying their specialty and availability.
2.  **Patient** views the list of available doctors.
3.  **Patient** selects an available time slot and books an appointment.
4.  The system confirms the booking and updates the doctor's schedule.


### Note :
* patient name is : "patient1@example.com" with static role "Patient"