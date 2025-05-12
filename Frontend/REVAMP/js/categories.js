document.addEventListener('DOMContentLoaded', () => {
    // Load categories when categories section is shown
    document.getElementById('categories-list').addEventListener('show', loadCategories);
    
    // Add category form submission
    const addCategoryForm = document.getElementById('addCategoryForm');
    if (addCategoryForm) {
        addCategoryForm.addEventListener('submit', handleAddCategory);
    }
    
    // Edit category form submission
    const editCategoryForm = document.getElementById('editCategoryForm');
    if (editCategoryForm) {
        editCategoryForm.addEventListener('submit', handleEditCategory);
    }
});

// categories.js
async function loadCategories() {
    try {
        const token = localStorage.getItem('token');
        const response = await fetch(`${API_CONFIG.BASE_URL}${API_CONFIG.ENDPOINTS.CATEGORIES}`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        
        if (!response.ok) {
            throw new Error('Failed to fetch categories');
        }
        
        const categories = await response.json();
        
        const categoriesTable = document.getElementById('categoriesTable');
        categoriesTable.innerHTML = '';
        
        categories.forEach(category => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${category.Id}</td>
                <td>${category.Name}</td>
                <td>${category.Description || '-'}</td>
                <td>
                    <button class="btn btn-sm btn-primary me-1" onclick="editCategory(${category.Id})">
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="deleteCategory(${category.Id})">
                        <i class="bi bi-trash"></i>
                    </button>
                </td>
            `;
            categoriesTable.appendChild(row);
        });
    } catch (error) {
        console.error('Failed to load categories:', error);
        alert(error.message);
    }
}

async function handleAddCategory(e) {
    e.preventDefault();
    
    const formData = {
        Name: document.getElementById('categoryName').value,
        Description: document.getElementById('categoryDescription').value
    };
    
    try {
        const token = localStorage.getItem('token');
        await makeRequest('/categories', 'POST', formData, token);
        
        alert('Category added successfully!');
        showSection('categories-list');
        loadCategories();
        loadCategoriesForDropdowns(); // Refresh dropdowns in product forms
    } catch (error) {
        console.error('Failed to add category:', error);
        alert(error.message);
    }
}

async function editCategory(categoryId) {
    try {
        const token = localStorage.getItem('token');
        const category = await makeRequest(`/categories/${categoryId}`, 'GET', null, token);
        
        // Populate the edit form
        document.getElementById('editCategoryId').value = category.Id;
        document.getElementById('editCategoryName').value = category.Name;
        document.getElementById('editCategoryDescription').value = category.Description || '';
        
        // Show the modal
        const modal = new bootstrap.Modal(document.getElementById('editCategoryModal'));
        modal.show();
    } catch (error) {
        console.error('Failed to load category for editing:', error);
        alert(error.message);
    }
}

async function handleEditCategory(e) {
    e.preventDefault();
    
    const categoryId = document.getElementById('editCategoryId').value;
    const formData = {
        Name: document.getElementById('editCategoryName').value,
        Description: document.getElementById('editCategoryDescription').value
    };
    
    try {
        const token = localStorage.getItem('token');
        await makeRequest(`/categories/${categoryId}`, 'PUT', formData, token);
        
        alert('Category updated successfully!');
        loadCategories();
        loadCategoriesForDropdowns(); // Refresh dropdowns in product forms
        
        // Hide the modal
        const modal = bootstrap.Modal.getInstance(document.getElementById('editCategoryModal'));
        modal.hide();
    } catch (error) {
        console.error('Failed to update category:', error);
        alert(error.message);
    }
}

async function deleteCategory(categoryId) {
    if (!confirm('Are you sure you want to delete this category?')) {
        return;
    }
    
    try {
        const token = localStorage.getItem('token');
        await makeRequest(`/categories/${categoryId}`, 'DELETE', null, token);
        
        alert('Category deleted successfully!');
        loadCategories();
        loadCategoriesForDropdowns(); // Refresh dropdowns in product forms
    } catch (error) {
        console.error('Failed to delete category:', error);
        alert(error.message);
    }
}

// Make functions available globally
window.editCategory = editCategory;
window.deleteCategory = deleteCategory;