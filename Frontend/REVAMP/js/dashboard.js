document.addEventListener('DOMContentLoaded', () => {
    // Sidebar toggle
    $('#sidebarCollapse').on('click', function () {
        $('#sidebar').toggleClass('active');
    });
    
    // Navigation
    function showSection(sectionId) {
        document.querySelectorAll('.section').forEach(section => {
            section.style.display = 'none';
        });
        document.getElementById(sectionId).style.display = 'block';
    }
    
    // Set up navigation links
    document.querySelectorAll('[href^="#"]').forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            const targetId = this.getAttribute('href').substring(1);
            showSection(targetId);
        });
    });
    
    // Default to dashboard view
    showSection('dashboard');
    
    // Load dashboard stats
    loadDashboardStats();
});

async function loadDashboardStats() {
    try {
        const token = localStorage.getItem('token');
        const stats = await makeRequest('/admin/dashboard', 'GET', null, token);
        
        document.getElementById('totalProducts').textContent = stats.TotalProducts;
        document.getElementById('totalCategories').textContent = stats.TotalCategories;
        document.getElementById('totalOrders').textContent = stats.TotalOrders;
        document.getElementById('totalUsers').textContent = stats.TotalUsers;
        
        const ordersTable = document.getElementById('recentOrders');
        ordersTable.innerHTML = '';
        
        stats.RecentOrders.forEach(order => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${order.Id}</td>
                <td>${order.UserEmail}</td>
                <td>${new Date(order.OrderDate).toLocaleDateString()}</td>
                <td>$${order.TotalAmount.toFixed(2)}</td>
                <td>${order.Status}</td>
            `;
            ordersTable.appendChild(row);
        });
    } catch (error) {
        console.error('Failed to load dashboard stats:', error);
    }
}

// Make showSection function available globally
window.showSection = showSection;