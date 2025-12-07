# 🚀 Admin Dashboard Setup and Testing Guide

## Quick Start

This guide will help you set up and test the Admin Dashboard functionality in SMART-HR.

---

## ✅ What Was Created

### 1. **New Files Created**

#### ViewModels
- `SMART-HR/ViewModels/AdminViewModels.cs`
  - AdminDashboardViewModel
  - UserListItemViewModel
  - CreateUserViewModel
  - EditUserViewModel
  - ResetPasswordViewModel
  - UserDetailsViewModel

#### Controller
- `SMART-HR/Controllers/AdminsController.cs` (Enhanced)
  - Dashboard
  - User Management (CRUD)
  - Password Reset
  - User Activation/Deactivation

#### Views
- `SMART-HR/Views/Admins/Dashboard.cshtml`
- `SMART-HR/Views/Admins/Users.cshtml`
- `SMART-HR/Views/Admins/CreateUser.cshtml`
- `SMART-HR/Views/Admins/EditUser.cshtml`
- `SMART-HR/Views/Admins/UserDetails.cshtml`
- `SMART-HR/Views/Admins/ResetPassword.cshtml`
- `SMART-HR/Views/Admins/DeleteUser.cshtml`
- `SMART-HR/Views/Admins/Settings.cshtml`

### 2. **Modified Files**

- `SMART-HR/Views/Shared/_Layout.cshtml`
  - Added Bootstrap Icons CDN
  - Added Admin link in navigation (visible only to Admin users)

- `SMART-HR/Views/Home/Index.cshtml`
  - Updated Admin module link to point to Dashboard
  - Added role-based conditional rendering

---

## 🔧 Prerequisites

Before testing, ensure:

1. ✅ Database is set up and migrations are applied
2. ✅ At least one Admin user exists in the database
3. ✅ Application compiles without errors
4. ✅ `IUtilisateurService` is properly registered in `Program.cs`

---

## 🎯 Testing Steps

### Step 1: Create an Admin User (If Not Already Created)

If you don't have an admin user, you can create one using the database directly or through the registration with a database update:

**Option A: Using SQL**
```sql
-- Insert a user
INSERT INTO Utilisateurs (Nom, Prenom, Email, MotDePasse, Role, Actif)
VALUES ('Admin', 'System', 'admin@smarthr.com', 'HASHED_PASSWORD', 'Admin', 1);

-- Get the UserId (let's say it's 1)
-- Insert admin record
INSERT INTO Admins (UtilisateurId, Departement, Poste, Telephone, EmailProfessionnel)
VALUES (1, 'Administration', 'Administrateur Système', '0600000000', 'admin@smarthr.com');
```

**Option B: Using Registration + Database Update**
1. Register a new user through the app
2. Update their role in the database to 'Admin'
3. Create an Admin record for them

---

### Step 2: Login as Admin

1. Navigate to `/Home/Login`
2. Enter admin credentials
3. You should see the **Admin** link in the navigation bar
4. You should now be logged in with Admin privileges

---

### Step 3: Access Admin Dashboard

1. Click **Admin** in the navigation bar, or
2. Navigate directly to `/Admins/Dashboard`
3. You should see:
   - Statistics cards (Total Users, Admins, RH, Managers, Employees)
   - Quick action buttons
   - Recent users list
   - System information

**Expected Result**: Dashboard loads successfully with statistics

---

### Step 4: Test User Management

#### 4.1 View All Users
1. Click **"Gérer les utilisateurs"** or navigate to `/Admins/Users`
2. Verify that all users are displayed in the table
3. Check that role badges and status indicators are correct

**Expected Result**: User list displays with proper formatting

#### 4.2 Search and Filter
1. In the Users page, try searching for a user by name
2. Try filtering by role (Admin, RH, Manager, Employe)
3. Try filtering by status (Active, Inactive)

**Expected Result**: List updates based on filters

---

### Step 5: Test User Creation

1. From Users page, click **"Créer un utilisateur"**
2. Fill in the form:
   ```
   Prénom: Test
   Nom: User
   Email: test.user@smarthr.com
   Mot de passe: Test123
   Rôle: Employe
   Compte actif: ✓
   Département: IT
   Poste: Testeur
   ```
3. If selecting "Employe" role, assign a manager
4. Click **"Créer l'utilisateur"**

**Expected Result**: 
- User is created successfully
- Success message appears
- Redirected to Users list
- New user appears in the list

---

### Step 6: Test User Editing

1. From Users list, click the **pencil icon** (Edit) for a user
2. Modify some information (e.g., change department)
3. Click **"Enregistrer les modifications"**

**Expected Result**:
- User information is updated
- Success message appears
- Changes are reflected in the user list

---

### Step 7: Test User Details

1. From Users list, click the **eye icon** (Details) for a user
2. Verify all information is displayed correctly
3. Check that role-specific information appears (e.g., manager for employees)

**Expected Result**: Complete user profile displays correctly

---

### Step 8: Test Password Reset

1. From Users list, click the **key icon** (Reset Password) for a user
2. Enter new password: `NewPassword123`
3. Confirm password: `NewPassword123`
4. Click **"Réinitialiser le mot de passe"**

**Expected Result**:
- Success message appears
- User can now log in with the new password

#### Verification
1. Logout
2. Try logging in as the user with the new password
3. Login should succeed

---

### Step 9: Test User Activation/Deactivation

1. From Users list, find an active user
2. Click the **red X icon** (Deactivate)
3. Confirm the action

**Expected Result**:
- User status changes to "Inactif"
- Badge turns red

#### Verification
1. Try logging in as the deactivated user
2. Login should fail or user should not be able to access the system

#### Reactivation
1. Click the **green check icon** (Activate)
2. Confirm the action

**Expected Result**: User status changes back to "Actif"

---

### Step 10: Test User Deletion

1. From Users list, click the **trash icon** (Delete) for a test user
2. Review the warning message
3. Confirm deletion

**Expected Result**:
- User is removed from the database
- Success message appears
- User no longer appears in the list

**⚠️ Warning**: Use a test user for this. Deletion is permanent!

---

### Step 11: Test System Settings

1. From Dashboard, click **"Paramètres"** or navigate to `/Admins/Settings`
2. Review the settings page
3. Note that most settings are placeholders for future development

**Expected Result**: Settings page loads with information cards

---

## 🔍 Troubleshooting

### Issue: Admin link doesn't appear in navigation

**Solution**:
- Check that you're logged in as a user with Role = "Admin"
- Verify session is storing UserRole correctly
- Check `_Layout.cshtml` for the Admin link condition

### Issue: "Access denied" message when accessing admin pages

**Solution**:
- Ensure your user has Role = "Admin" in the database
- Check that the session contains UserRole = "Admin"
- Verify `IsAdmin()` method in AdminsController

### Issue: Cannot create user - email already exists

**Solution**:
- Use a unique email address
- Check if the email is already registered in the Utilisateurs table

### Issue: Manager dropdown is empty when creating employee

**Solution**:
- Ensure you have at least one Manager in the system
- Create a Manager user first, then create Employee

### Issue: Role-specific entity not created

**Solution**:
- Check that the switch statement in CreateUser action works correctly
- Verify database relationships are properly configured

---

## 🧪 Test Scenarios

### Scenario 1: Complete User Lifecycle

1. ✅ Create a new Employee user
2. ✅ View user details
3. ✅ Edit user information
4. ✅ Reset user password
5. ✅ Deactivate user
6. ✅ Reactivate user
7. ✅ Delete user

### Scenario 2: Role Management

1. ✅ Create users with all different roles (Admin, RH, Manager, Employe)
2. ✅ Verify role-specific entities are created
3. ✅ Change a user's role from Employee to Manager
4. ✅ Verify role-specific fields appear/disappear correctly

### Scenario 3: Security Validation

1. ✅ Try accessing admin pages as non-admin user (should fail)
2. ✅ Try creating user with duplicate email (should fail)
3. ✅ Try creating user with short password (should fail)
4. ✅ Deactivate a user and verify they cannot log in

---

## 📊 Expected Dashboard Statistics

After creating test users, your dashboard should show:

```
Total Users: X
Active Users: Y
Inactive Users: Z

Admins: A
RH: B
Managers: C
Employees: D
```

Where X = A + B + C + D

---

## 🎨 Visual Checks

### Navigation Bar
- [ ] Admin link appears for admin users
- [ ] Admin link has speedometer icon
- [ ] Admin link is hidden for non-admin users

### Dashboard
- [ ] Statistics cards display correct numbers
- [ ] Cards have appropriate colors (primary, danger, success, warning, info)
- [ ] Icons render correctly
- [ ] Quick action buttons work

### User List
- [ ] Table is responsive
- [ ] Role badges have correct colors
- [ ] Status badges show correctly
- [ ] Action buttons are visible and functional
- [ ] Search and filters work

### Forms
- [ ] Form validation works
- [ ] Required fields are marked
- [ ] Error messages display clearly
- [ ] Success messages appear after actions

---

## ✨ Features Checklist

- [x] Admin Dashboard with statistics
- [x] User listing with search and filters
- [x] Create new users with role assignment
- [x] Edit existing users
- [x] View user details
- [x] Reset user passwords
- [x] Activate/Deactivate users
- [x] Delete users with confirmation
- [x] Role-based access control
- [x] Settings page (placeholder)
- [x] Responsive design
- [x] Bootstrap Icons integration
- [x] Success/Error notifications
- [x] Breadcrumb navigation

---

## 🚀 Next Steps

After testing the basic functionality, you can:

1. **Enhance Security**
   - Add password complexity requirements
   - Implement two-factor authentication
   - Add audit logging

2. **Improve User Management**
   - Add bulk user operations
   - Add user import/export (CSV)
   - Add user activity logs

3. **Implement System Settings**
   - Make settings functional (not just placeholders)
   - Add configuration for email, leave management, etc.

4. **Add Analytics**
   - User activity reports
   - System usage statistics
   - Performance metrics

5. **Notifications**
   - Email notifications for password resets
   - Account activation/deactivation notifications
   - System alerts

---

## 📝 Notes

- The Admin Dashboard is designed to be the central control panel
- All admin operations are logged through TempData success/error messages
- Deactivation is preferred over deletion for data integrity
- The interface is responsive and mobile-friendly
- Bootstrap Icons provide a modern, consistent look

---

## ❓ Questions?

If you encounter any issues or have questions:

1. Check the `ADMIN_FEATURES.md` file for detailed documentation
2. Review the inline code comments in the controllers and views
3. Check the console for any errors
4. Verify database schema and relationships

---

**Happy Testing! 🎉**

The Admin Dashboard provides you with powerful tools to manage your SMART-HR system. Use them wisely!

