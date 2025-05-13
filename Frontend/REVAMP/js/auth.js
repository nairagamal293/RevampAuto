const API_BASE_URL = 'https://localhost:7058/api';

// Login handler
document.getElementById('loginForm')?.addEventListener('submit', async function(e) {
    e.preventDefault();
    
    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;
    
    try {
        const response = await fetch(`${API_BASE_URL}/auth/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                email,
                password
            })
        });
        
        const data = await response.json();
        console.log('Login response:', data); // Debug
        
        if (response.ok) {
    localStorage.setItem('authToken', data.token);
    localStorage.setItem('userEmail', email);

    // Store user info for navbar display
    localStorage.setItem('user', JSON.stringify({
        name: email // or fetch/display full name if available from API later
    }));

    // Decode roles from token
    const tokenPayload = JSON.parse(atob(data.token.split('.')[1]));
    const roles = tokenPayload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 
                 tokenPayload.role || 
                 [];
    const userRoles = Array.isArray(roles) ? roles : [roles];
    localStorage.setItem('userRoles', JSON.stringify(userRoles));

    // Redirect based on role
    if (userRoles.includes('Admin')) {
        window.location.href = 'admin.html';
    } else {
        window.location.href = 'index.html';
    }
        } else {
            alert(data.message || 'Login failed');
        }
    } catch (error) {
        console.error('Login error:', error);
        alert('An error occurred during login');
    }
});


// Signup handler (unchanged)
document.getElementById('signupForm')?.addEventListener('submit', async function(e) {
    e.preventDefault();
    
    const userData = {
        firstName: document.getElementById('firstName').value,
        lastName: document.getElementById('lastName').value,
        email: document.getElementById('email').value,
        password: document.getElementById('password').value,
        address: document.getElementById('address').value,
        phoneNumber: document.getElementById('phoneNumber').value
    };
    
    try {
        const response = await fetch(`${API_BASE_URL}/auth/register`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(userData)
        });
        
        if (response.ok) {
            alert('Registration successful! Please login.');
            window.location.href = 'login.html';
        } else {
            const errorData = await response.json();
            alert(errorData.errors?.join('\n') || 'Registration failed');
        }
    } catch (error) {
        console.error('Registration error:', error);
        alert('An error occurred during registration');
    }
});

// Authentication check
function checkAuth() {
    const token = localStorage.getItem('authToken');
    const roles = JSON.parse(localStorage.getItem('userRoles') || []);
    
    console.log('Auth check - Roles:', roles); // Debug
    
    if (token) {
        // If on login page, redirect to appropriate page
        if (window.location.pathname.endsWith('login.html')) {
            const isAdmin = roles.includes('Admin');
            window.location.href = isAdmin ? 'admin.html' : 'index.html';
        }
    } else if (!window.location.pathname.endsWith('login.html')) {
        // Redirect to login if not authenticated
        window.location.href = 'login.html';
    }
}

// Initialize auth check
window.addEventListener('DOMContentLoaded', checkAuth);