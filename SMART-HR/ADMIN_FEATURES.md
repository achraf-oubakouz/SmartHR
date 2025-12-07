# 🛡️ SMART-HR - Admin Dashboard Documentation

## Overview

The Admin Dashboard is the central control center of the SMART-HR application. It provides administrators with complete control over user management, system configuration, and access to all management modules.

## 🔐 Access Control

- **Role Required**: Admin
- **Access URL**: `/Admins/Dashboard`
- **Navigation**: The Admin link appears in the top navigation bar only for users with the Admin role

If a non-admin user tries to access admin pages, they will be redirected to the home page with an error message.

## 📊 Features

### 1. Dashboard (`/Admins/Dashboard`)

The main admin dashboard provides:

#### Statistics Overview
- **Total Users**: Count of all users in the system
- **Active Users**: Number of currently active users
- **Inactive Users**: Number of deactivated users
- **Role Breakdown**: 
  - Admins
  - RH (Human Resources)
  - Managers
  - Employees

#### Quick Actions
- Manage Users
- Create New User
- Access Calendar
- System Settings
- Leave Requests Management
- Reports
- Documents
- Employee Management

#### Recent Users
- Display of the 5 most recently created users
- Quick view of name, email, role, and status

#### System Information
- System version
- Database connection status
- Quick links to other management sections

---

### 2. User Management (`/Admins/Users`)

Complete user management interface with advanced features:

#### Search and Filter
- **Search**: By name, first name, or email
- **Role Filter**: Filter by Admin, RH, Manager, or Employee
- **Status Filter**: Filter by Active or Inactive users

#### User List Table
Displays all users with:
- Full name
- Email address
- Role (with color-coded badges)
- Status (Active/Inactive)
- Action buttons

#### Available Actions per User
- **View Details** 👁️ - View complete user information
- **Edit** ✏️ - Modify user information and role
- **Reset Password** 🔑 - Reset user password
- **Activate/Deactivate** ⭕/❌ - Toggle user active status
- **Delete** 🗑️ - Permanently remove user

---

### 3. Create User (`/Admins/CreateUser`)

Create new users with role assignment:

#### Basic Information
- First Name (required)
- Last Name (required)
- Email (required, unique)
- Password (required, minimum 6 characters)
- Role (required): Admin, RH, Manager, or Employee
- Active Status (checkbox, default: active)

#### Professional Information
- Department
- Position
- Phone
- Professional Email

#### Role-Specific Fields
- **For Employees**: Option to assign a manager

#### Automatic Role Entity Creation
When creating a user, the system automatically:
1. Creates the base user account
2. Creates the corresponding role-specific entity (Admin, RH, Manager, or Employee)
3. Sets appropriate default values for the role

---

### 4. Edit User (`/Admins/EditUser/:id`)

Modify existing user information:

#### Editable Fields
- All basic and professional information
- Role (can change user's role)
- Active status
- Manager assignment (for employees)

#### Role Change Handling
- When changing a user's role, the system updates the corresponding role-specific entity
- Original role entities are preserved but the user's active role is updated

#### Quick Actions
- Reset password
- Activate/Deactivate account
- Delete user

---

### 5. User Details (`/Admins/UserDetails/:id`)

View complete user profile:

#### Information Displayed
- Full name and email
- Role (with badge)
- Active status
- Department and position
- Phone and professional email
- **For Employees**: Assigned manager
- **For Managers**: Number of supervised employees

#### Available Actions
- Edit user
- Reset password
- Activate/Deactivate
- Delete user
- Return to user list

---

### 6. Reset Password (`/Admins/ResetPassword/:id`)

Securely reset user passwords:

#### Features
- User identification display (name and email)
- New password field (minimum 6 characters)
- Password confirmation field
- Security warnings and tips

#### Security Tips Provided
- Use temporary passwords
- Don't share via unsecured email
- Use strong passwords with mixed characters
- Avoid personal information

---

### 7. Delete User (`/Admins/DeleteUser/:id`)

Permanently delete users with safeguards:

#### Safety Features
- Warning alert about irreversible action
- Complete user information display
- Manager dependency warning (if applicable)
- Alternative suggestion (deactivation)
- Confirmation dialog

#### What Gets Deleted
1. User base account
2. Role-specific entity (Admin, RH, Manager, or Employee)
3. All associated session data

---

### 8. Activate/Deactivate User

Toggle user access without deleting:

#### Deactivation
- Prevents user from logging in
- Preserves all user data and history
- Can be reactivated at any time

#### Benefits
- Recommended over deletion
- Maintains data integrity
- Reversible action
- Useful for temporary suspensions

---

### 9. System Settings (`/Admins/Settings`)

Configuration and system management:

#### Current Features (Placeholder)
- General Settings
  - Application name
  - Default language
  - Time zone

- Security Settings
  - Minimum password length
  - Two-factor authentication (coming soon)
  - Session duration

- Email Settings
  - SMTP server configuration
  - Email notifications
  - Sender email

- Leave Management
  - Annual leave days
  - Automatic approval
  - Leave types management

- Database
  - Connection status
  - Automatic backup configuration
  - Manual backup

- Quick Links
  - Calendar management
  - Employee view
  - Managers view
  - Reports

> **Note**: Most settings are currently placeholders for future development.

---

## 🎨 User Interface Features

### Color-Coded Role Badges
- **Admin**: Red badge
- **RH**: Blue badge
- **Manager**: Yellow badge
- **Employee**: Light blue badge

### Status Indicators
- **Active**: Green badge
- **Inactive**: Red badge

### Responsive Design
- Mobile-friendly interface
- Bootstrap 5 components
- Bootstrap Icons for visual clarity
- Card-based layout for better organization

---

## 🔒 Security Features

1. **Role-Based Access Control**
   - Only Admin users can access admin pages
   - Automatic redirection for unauthorized access

2. **Password Management**
   - Password hashing for security
   - Minimum length requirements
   - Secure password reset process

3. **User Status Control**
   - Ability to deactivate users
   - Prevents unauthorized access

4. **Confirmation Dialogs**
   - Critical actions require confirmation
   - Prevents accidental deletions or modifications

---

## 📱 Navigation

### Admin Access Points

1. **Top Navigation Bar**
   - "Admin" link (visible only to Admin users)
   - Direct access to Dashboard

2. **Home Page**
   - "Administration" module card (Admin users only)
   - Links to Dashboard

3. **Footer**
   - "Dashboard Admin" link in Espace RH section (Admin users only)

---

## 🚀 Usage Examples

### Creating a New Employee

1. Navigate to `Admins` → `Users` → `Create User`
2. Fill in basic information:
   - First Name: "Jean"
   - Last Name: "Dupont"
   - Email: "jean.dupont@company.com"
   - Password: (secure password)
   - Role: "Employe"
3. Add professional information:
   - Department: "IT"
   - Position: "Developer"
   - Phone: "0612345678"
4. Assign a manager (if applicable)
5. Click "Create User"

### Resetting a User's Password

1. Navigate to `Admins` → `Users`
2. Find the user in the list
3. Click the key icon 🔑 or go to user details and click "Reset Password"
4. Enter new password (minimum 6 characters)
5. Confirm password
6. Click "Reset Password"
7. Communicate the new password securely to the user

### Temporarily Suspending a User

1. Navigate to `Admins` → `Users`
2. Find the user in the list
3. Click the red deactivate button ❌
4. Confirm the action
5. User is now unable to log in but data is preserved
6. To reactivate, click the green activate button ✅

---

## 🛠️ Technical Details

### ViewModels

- **AdminDashboardViewModel**: Dashboard statistics and data
- **UserListItemViewModel**: User list item display
- **CreateUserViewModel**: User creation form
- **EditUserViewModel**: User editing form
- **ResetPasswordViewModel**: Password reset form
- **UserDetailsViewModel**: User details display

### Controller Actions

- `Dashboard()` - Main dashboard
- `Users()` - List all users with filters
- `CreateUser()` - GET/POST create user
- `EditUser(id)` - GET/POST edit user
- `UserDetails(id)` - View user details
- `ResetPassword(id)` - GET/POST reset password
- `DeleteUser(id)` - GET/POST delete user
- `ToggleActive(id)` - POST toggle user status
- `Settings()` - System settings (placeholder)

### Database Impact

The admin operations interact with:
- `Utilisateurs` table (base user data)
- `Admins` table (admin-specific data)
- `RessourcesHumaines` table (RH-specific data)
- `Managers` table (manager-specific data)
- `Employes` table (employee-specific data)

---

## 🎯 Best Practices

1. **User Management**
   - Always verify email uniqueness before creating users
   - Use strong, temporary passwords and require users to change them
   - Prefer deactivation over deletion for data integrity

2. **Role Assignment**
   - Carefully consider role assignments
   - Limit Admin role to essential personnel
   - Assign managers appropriately for proper workflow

3. **Security**
   - Regularly review active users
   - Deactivate unused accounts
   - Monitor user activities through the dashboard

4. **Communication**
   - Always inform users when their accounts are modified
   - Provide clear instructions for password changes
   - Keep users informed of role changes

---

## 🔮 Future Enhancements

Potential features for future development:

1. **Advanced User Management**
   - Bulk user operations
   - CSV import/export
   - User activity logs

2. **System Settings**
   - Fully functional settings pages
   - Email configuration
   - Backup management
   - System logs

3. **Analytics**
   - User activity analytics
   - System usage reports
   - Performance metrics

4. **Notifications**
   - Email notifications for user actions
   - System alerts
   - Automated reminders

5. **Audit Trail**
   - Complete action history
   - Change logs
   - Compliance reports

---

## 📞 Support

For issues or questions about the Admin Dashboard:
- Check the inline help and tooltips
- Review this documentation
- Contact the system administrator

---

## 📝 Version Information

- **Version**: 1.0
- **Last Updated**: December 2025
- **Framework**: ASP.NET Core 8.0
- **UI Framework**: Bootstrap 5
- **Icons**: Bootstrap Icons 1.11.0

---

## ✅ Checklist for Admins

### Daily Tasks
- [ ] Review recent user registrations
- [ ] Check for pending access requests
- [ ] Monitor system status

### Weekly Tasks
- [ ] Review active users list
- [ ] Check for inactive accounts
- [ ] Review role assignments

### Monthly Tasks
- [ ] Audit user permissions
- [ ] Review system usage statistics
- [ ] Update system settings as needed
- [ ] Backup user data

---

**Remember**: With great power comes great responsibility. The Admin Dashboard gives you complete control over the system. Always double-check before performing destructive actions.

