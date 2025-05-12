// Base API URL - adjust according to your environment
const API_BASE_URL = 'https://localhost:7058/api';
const BASE_URL = 'https://localhost:7058'; // Add this for static files
let currentProductId = null;
let currentCategoryId = null;

// Initialize the dashboard
document.addEventListener('DOMContentLoaded', function() {
    const token = localStorage.getItem('authToken');
    const roles = JSON.parse(localStorage.getItem('userRoles') || '[]');
    
    console.log('Admin page check - Roles:', roles); // Debug
    
    if (!token || !roles.includes('Admin')) {
        console.log('Redirecting to index - not admin');
        window.location.href = 'index.html';
        return;
    }

    // Set user email
    const userEmail = localStorage.getItem('userEmail');
    document.getElementById('userEmail').textContent = userEmail;
     document.getElementById('viewUsersLink').addEventListener('click', showUsersManagement);

    
    // Load dashboard data
    loadDashboardData();
    
    // Setup event listeners
    setupEventListeners();
    
    // Initialize sidebar toggle - using vanilla JS instead of jQuery
    document.getElementById('sidebarCollapse').addEventListener('click', function() {
        document.getElementById('sidebar').classList.toggle('active');
    });
   


     console.log('User is admin, loading dashboard');
});

function setupEventListeners() {
    // Navigation links
    document.getElementById('viewProductsLink').addEventListener('click', showProductsManagement);
    document.getElementById('addProductLink').addEventListener('click', showAddProductForm);
    document.getElementById('viewCategoriesLink').addEventListener('click', showCategoriesManagement);
    document.getElementById('addCategoryLink').addEventListener('click', showAddCategoryForm);
    
    // Buttons
    document.getElementById('addNewProductBtn').addEventListener('click', showAddProductForm);
    document.getElementById('cancelProductBtn').addEventListener('click', showProductsManagement);
    document.getElementById('addNewCategoryBtn').addEventListener('click', showAddCategoryForm);
    document.getElementById('cancelCategoryBtn').addEventListener('click', showCategoriesManagement);
    document.getElementById('logoutBtn').addEventListener('click', logout);
    
    // Forms
    document.getElementById('productForm').addEventListener('submit', handleProductFormSubmit);
    document.getElementById('categoryForm').addEventListener('submit', handleCategoryFormSubmit);
}

async function loadDashboardData() {
    try {
        const headers = getAuthHeaders();
        
        // Load counts
        const productsCount = await fetch(`${API_BASE_URL}/products`, { headers });
        const categoriesCount = await fetch(`${API_BASE_URL}/categories`, { headers });
        const ordersCount = await fetch(`${API_BASE_URL}/admin/Admin/dashboard`, { headers });
        const usersCount = await fetch(`${API_BASE_URL}/admin/Admin/dashboard`, { headers });
        
        const productsData = await productsCount.json();
        const categoriesData = await categoriesCount.json();
        const dashboardData = await ordersCount.json();
        
        document.getElementById('productsCount').textContent = productsData.length;
        document.getElementById('categoriesCount').textContent = categoriesData.length;
        document.getElementById('ordersCount').textContent = dashboardData.totalOrders || 0;
        document.getElementById('usersCount').textContent = dashboardData.totalUsers || 0;
        
        // Load recent orders
        if (dashboardData.recentOrders) {
            const ordersTable = document.getElementById('recentOrdersTable');
            ordersTable.innerHTML = '';
            
            dashboardData.recentOrders.forEach(order => {
                const row = document.createElement('tr');
                row.innerHTML = `
                    <td>${order.id}</td>
                    <td>${order.userEmail}</td>
                    <td>${new Date(order.orderDate).toLocaleDateString()}</td>
                    <td>$${order.totalAmount.toFixed(2)}</td>
                    <td>${order.status}</td>
                `;
                ordersTable.appendChild(row);
            });
        }
    } catch (error) {
        console.error('Error loading dashboard data:', error);
    }
}

async function showProductsManagement() {
    hideAllSections();
    document.getElementById('productsManagement').style.display = 'block';
    
    try {
        const headers = getAuthHeaders();
        const response = await fetch(`${API_BASE_URL}/Products`, { headers });
        const products = await response.json();
        
        const productsTable = document.getElementById('productsTable').querySelector('tbody');
        productsTable.innerHTML = '';
        
        products.forEach(product => {
    const row = document.createElement('tr');
    row.innerHTML = `
    <td>${product.id}</td>
    <td>
        ${product.imageUrls && product.imageUrls.length > 0 ? 
            `<div id="carousel-${product.id}" class="carousel slide" style="width: 80px;">
                <div class="carousel-inner">
                    ${product.imageUrls.map((img, index) => `
                        <div class="carousel-item ${index === 0 ? 'active' : ''}">
                            <img src="${BASE_URL}${img}" class="d-block w-100" alt="${product.name}">
                        </div>
                    `).join('')}
                </div>
            </div>` : 
            '<span class="text-muted">No image</span>'}
    </td>
                <td>${product.name}</td>
                <td>${product.categoryName}</td>
                <td>$${product.price.toFixed(2)}</td>
                <td>${product.stockQuantity}</td>
                <td>
                    <button class="btn btn-sm btn-primary edit-product" data-id="${product.id}">
    <i class="bi bi-pencil"></i>
</button>
                    <button class="btn btn-sm btn-danger delete-product" data-id="${product.id}">
                        <i class="bi bi-trash"></i>
                    </button>
                    <button class="btn btn-sm btn-info manage-images" data-id="${product.id}">
                        <i class="bi bi-images"></i>
                    </button>
                </td>
            `;
            productsTable.appendChild(row);
        });
        
        // Add event listeners to action buttons
// In your showProductsManagement function, update the event listeners:

// Fix your event listeners like this:
document.querySelectorAll('.edit-product').forEach(btn => {
    btn.addEventListener('click', function(e) {
        e.preventDefault(); // Add this
        const productId = this.getAttribute('data-id');
        showAddProductForm(productId);
    });
});

document.querySelectorAll('.delete-product').forEach(btn => {
    btn.addEventListener('click', (e) => {
        e.preventDefault();
        deleteProduct(btn.dataset.id);
    });
});
        document.querySelectorAll('.delete-product').forEach(btn => {
            btn.addEventListener('click', (e) => {
        e.preventDefault(); deleteProduct(btn.dataset.id); });
        });
        
        document.querySelectorAll('.manage-images').forEach(btn => {
            btn.addEventListener('click', (e) => {
        e.preventDefault(); manageProductImages(btn.dataset.id); });
        });
    } catch (error) {
        console.error('Error loading products:', error);
        alert('Failed to load products');
    }
}

async function showAddProductForm(productId = null) {
    hideAllSections();
    document.getElementById('productFormContainer').style.display = 'block';
    
    // Reset form
    const form = document.getElementById('productForm');
    form.reset();
    document.getElementById('imagePreview').innerHTML = '';
    document.getElementById('productId').value = '';
    document.getElementById('productImages').value = '';
    
    document.getElementById('productFormTitle').textContent = productId ? 'Edit Product' : 'Add New Product';

    if (productId) {
        try {
            const headers = getAuthHeaders();
            const response = await fetch(`${API_BASE_URL}/products/${productId}`, { headers });
            
            if (!response.ok) {
                throw new Error(`Server responded with ${response.status}`);
            }
            
            const product = await response.json();
            
            // Populate form fields
            document.getElementById('productId').value = product.id;
            document.getElementById('productName').value = product.name || '';
            document.getElementById('productDescription').value = product.description || '';
            document.getElementById('productPrice').value = product.price || '';
            document.getElementById('productStock').value = product.stockQuantity || '';
            
            // Load images
            const imagePreview = document.getElementById('imagePreview');
            if (product.imageUrls?.length > 0) {
                product.imageUrls.forEach(url => {
                    const imgDiv = document.createElement('div');
                    imgDiv.className = 'image-preview-item';
                    imgDiv.innerHTML = `
                        <img src="${BASE_URL}${url}" class="img-thumbnail" style="height: 100px;">
                        <button class="btn btn-sm btn-danger remove-image" data-url="${url}">
                            <i class="bi bi-x"></i>
                        </button>
                    `;
                    imagePreview.appendChild(imgDiv);
                });
            }
            
            await loadCategoriesDropdown(product.categoryId);
        } catch (error) {
            console.error('Error loading product:', error);
            alert('Failed to load product data. Starting with empty form.');
            await loadCategoriesDropdown();
        }
    } else {
        // For new products
        await loadCategoriesDropdown();
    }
}

async function loadCategoriesDropdown(selectedId = null) {
    try {
        const headers = getAuthHeaders();
        const response = await fetch(`${API_BASE_URL}/Categories`, { headers });
        
        if (!response.ok) {
            throw new Error(`Failed to fetch categories: ${response.status}`);
        }
        
        const categories = await response.json();
        
        const categoryDropdown = document.getElementById('productCategory');
        categoryDropdown.innerHTML = '<option value="">Select a category</option>';
        
        categories.forEach(category => {
            const option = document.createElement('option');
            option.value = category.id;
            option.textContent = category.name;
            if (selectedId && category.id == selectedId) {
                option.selected = true;
            }
            categoryDropdown.appendChild(option);
        });
    } catch (error) {
        console.error('Error loading categories:', error);
        // Optionally show an error message to the user
    }
}

async function handleProductFormSubmit(e) {
    e.preventDefault();
    const submitButton = e.target.querySelector('button[type="submit"]');
    const originalText = submitButton.innerHTML;
    
    try {
        // UI feedback
        submitButton.disabled = true;
        submitButton.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Saving...';

        // Prepare data with validation
        const productData = {
            name: document.getElementById('productName').value.trim(),
            description: document.getElementById('productDescription').value.trim() || null,
            price: parseFloat(document.getElementById('productPrice').value),
            stockQuantity: parseInt(document.getElementById('productStock').value),
            categoryId: parseInt(document.getElementById('productCategory').value)
        };

        // Validation
        if (!productData.name) throw new Error('Product name is required');
        if (isNaN(productData.price) || productData.price <= 0) throw new Error('Invalid price');
        if (isNaN(productData.stockQuantity) || productData.stockQuantity < 0) throw new Error('Invalid stock quantity');
        if (isNaN(productData.categoryId) || productData.categoryId <= 0) throw new Error('Invalid category');

        const productId = document.getElementById('productId').value;
        const isEdit = !!productId;
        const endpoint = `${API_BASE_URL}/products${isEdit ? `/${productId}` : ''}`;
        const method = isEdit ? 'PUT' : 'POST';
        
        const response = await fetch(endpoint, {
            method: method,
            headers: {
                ...getAuthHeaders(),
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(productData)
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(errorText || `Failed to ${isEdit ? 'update' : 'create'} product`);
        }

        // For POST requests, get the ID from the response
        let savedProductId = productId;
        if (!isEdit) {
            const createdProduct = await response.json();
            savedProductId = createdProduct.id;
            if (!savedProductId) {
                throw new Error('Failed to get product ID from response');
            }
        }

        // Handle image uploads if any
        const imageInput = document.getElementById('productImages');
        if (imageInput.files.length > 0) {
            try {
                await uploadProductImages(savedProductId, imageInput.files, true); // Set first image as main for new products
            } catch (uploadError) {
                console.error('Image upload failed:', uploadError);
                alert('Product saved but images failed to upload');
            }
        }

        alert(`Product ${isEdit ? 'updated' : 'created'} successfully!`);
        showProductsManagement();
    } catch (error) {
        console.error('Form error:', error);
        alert(`Error: ${error.message}`);
    } finally {
        submitButton.disabled = false;
        submitButton.innerHTML = originalText;
    }
}



async function uploadProductImages(productId, files, setMain = false) {
    try {
        const headers = getAuthHeaders();
        const formData = new FormData();
        
        for (let i = 0; i < files.length; i++) {
            formData.append('files', files[i]);
        }
        
        const response = await fetch(`${API_BASE_URL}/products/${productId}/images?setMain=${setMain}`, {
            method: 'POST',
            headers: {
                'Authorization': headers.Authorization
            },
            body: formData
        });
        
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(errorText || 'Image upload failed');
        }
        
        return await response.json();
    } catch (error) {
        console.error('Image upload error:', error);
        throw error;
    }
}



async function editProduct(productId) {
    if (!productId) {
        console.error('No product ID provided for edit');
        return;
    }
    await showAddProductForm(productId);
}

async function deleteProduct(productId) {
    if (!confirm('Are you sure you want to delete this product?')) return;
    
    try {
        const headers = getAuthHeaders();
        const response = await fetch(`${API_BASE_URL}/products/${productId}`, {
            method: 'DELETE',
            headers
        });
        
        if (response.ok) {
            alert('Product deleted successfully!');
            showProductsManagement();
        } else {
            const errorData = await response.json();
            alert(errorData.message || 'Failed to delete product');
        }
    } catch (error) {
        console.error('Error deleting product:', error);
        alert('An error occurred while deleting the product');
    }
}

async function manageProductImages(productId) {
    currentProductId = productId;
    
    try {
        const headers = getAuthHeaders();
        const response = await fetch(`${API_BASE_URL}/products/${productId}`, { headers });
        const product = await response.json();
        
        const currentImagesContainer = document.getElementById('currentImages');
        currentImagesContainer.innerHTML = '';
        
        if (product.imageUrls && product.imageUrls.length > 0) {
            product.imageUrls.forEach((imageUrl, index) => {
                const imgCard = document.createElement('div');
                imgCard.className = 'card';
                imgCard.style.width = '150px';
                imgCard.innerHTML = `
                    <img src="${BASE_URL}${imageUrl}" class="card-img-top" style="height: 100px; object-fit: cover;">
                    <div class="card-body p-2">
                        <div class="form-check">
                            <input class="form-check-input set-main-image" type="radio" name="mainImage" 
                                   id="mainImage${index}" ${product.mainImageUrl === imageUrl ? 'checked' : ''}
                                   value="${imageUrl}">
                            <label class="form-check-label" for="mainImage${index}">
                                Main Image
                            </label>
                        </div>
                        <button class="btn btn-sm btn-danger w-100 delete-image" data-url="${imageUrl}">
                            <i class="bi bi-trash"></i> Delete
                        </button>
                    </div>
                `;
                currentImagesContainer.appendChild(imgCard);
            });
        }
        
        // Initialize modal
        const modal = new bootstrap.Modal(document.getElementById('productImagesModal'));
        modal.show();
    } catch (error) {
        console.error('Error loading product images:', error);
        alert('Failed to load product images');
    }
}

function resetProductForm() {
    const form = document.getElementById('productForm');
    form.reset();
    document.getElementById('imagePreview').innerHTML = '';
    document.getElementById('productId').value = '';
    document.getElementById('productImages').value = '';
}

async function fetchWithErrorHandling(url, options = {}) {
    try {
        const response = await fetch(url, options);
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(errorText || `HTTP error! status: ${response.status}`);
        }
        return await response.json();
    } catch (error) {
        console.error(`Fetch error for ${url}:`, error);
        throw error;
    }
}


document.getElementById('saveImagesBtn')?.addEventListener('click', async function() {
    try {
        const headers = getAuthHeaders();
        const mainImageUrl = document.querySelector('input[name="mainImage"]:checked')?.value;
        
        // Set main image if selected
        if (mainImageUrl) {
            const imageId = mainImageUrl.split('/').pop().split('.')[0]; // Extract image ID from URL
            await fetch(`${API_BASE_URL}/products/${currentProductId}/images/${imageId}/setmain`, {
                method: 'PUT',
                headers
            });
        }
        
        // Handle new image uploads
        const newImagesInput = document.getElementById('newProductImages');
        if (newImagesInput.files.length > 0) {
            const formData = new FormData();
            for (let i = 0; i < newImagesInput.files.length; i++) {
                formData.append('files', newImagesInput.files[i]);
            }
            
            await fetch(`${API_BASE_URL}/products/${currentProductId}/images`, {
                method: 'POST',
                headers: {
                    'Authorization': headers.Authorization
                },
                body: formData
            });
        }
        
        // Handle image deletions
        const deletedImages = document.querySelectorAll('.delete-image');
        for (const btn of deletedImages) {
            if (btn.classList.contains('confirmed')) {
                const imageUrl = btn.dataset.url;
                const imageId = imageUrl.split('/').pop().split('.')[0]; // Extract image ID from URL
                
                await fetch(`${API_BASE_URL}/products/${currentProductId}/images/${imageId}`, {
                    method: 'DELETE',
                    headers
                });
            }
        }
        
        alert('Images updated successfully!');
        const modal = bootstrap.Modal.getInstance(document.getElementById('productImagesModal'));
        modal.hide();
        showProductsManagement();
    } catch (error) {
        console.error('Error saving images:', error);
        alert('Failed to save images');
    }
});

// Categories Management
async function showCategoriesManagement() {
    hideAllSections();
    document.getElementById('categoriesManagement').style.display = 'block';
    
    try {
        const headers = getAuthHeaders();
        const response = await fetch(`${API_BASE_URL}/categories`, { headers });
        const categories = await response.json();
        
        const categoriesTable = document.getElementById('categoriesTable').querySelector('tbody');
        categoriesTable.innerHTML = '';
        
        categories.forEach(category => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${category.id}</td>
                <td>${category.name}</td>
                <td>${category.description || '-'}</td>
                <td>
                    <button class="btn btn-sm btn-primary edit-category" data-id="${category.id}">
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm btn-danger delete-category" data-id="${category.id}">
                        <i class="bi bi-trash"></i>
                    </button>
                </td>
            `;
            categoriesTable.appendChild(row);
        });
        
        // Add event listeners to action buttons
        document.querySelectorAll('.edit-category').forEach(btn => {
            btn.addEventListener('click', () => editCategory(btn.dataset.id));
        });
        
        document.querySelectorAll('.delete-category').forEach(btn => {
            btn.addEventListener('click', () => deleteCategory(btn.dataset.id));
        });
    } catch (error) {
        console.error('Error loading categories:', error);
        alert('Failed to load categories');
    }
}

async function showAddCategoryForm(categoryId = null) {
    hideAllSections();
    document.getElementById('categoryFormContainer').style.display = 'block';
    
    if (categoryId) {
        document.getElementById('categoryFormTitle').textContent = 'Edit Category';
        currentCategoryId = categoryId;
        
        try {
            const headers = getAuthHeaders();
            const response = await fetch(`${API_BASE_URL}/categories/${categoryId}`, { headers });
            const category = await response.json();
            
            document.getElementById('categoryId').value = category.id;
            document.getElementById('categoryName').value = category.name;
            document.getElementById('categoryDescription').value = category.description || '';
        } catch (error) {
            console.error('Error loading category:', error);
            alert('Failed to load category data');
        }
    } else {
        document.getElementById('categoryFormTitle').textContent = 'Add New Category';
        currentCategoryId = null;
        document.getElementById('categoryForm').reset();
    }
}

async function handleCategoryFormSubmit(e) {
    e.preventDefault();
    
    const categoryData = {
        name: document.getElementById('categoryName').value,
        description: document.getElementById('categoryDescription').value
    };
    
    const categoryId = document.getElementById('categoryId').value;
    const isEdit = !!categoryId;
    
    try {
        const headers = getAuthHeaders();
        let response;
        
        if (isEdit) {
            response = await fetch(`${API_BASE_URL}/categories/${categoryId}`, {
                method: 'PUT',
                headers: {
                    ...headers,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(categoryData)
            });
        } else {
            response = await fetch(`${API_BASE_URL}/categories`, {
                method: 'POST',
                headers: {
                    ...headers,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(categoryData)
            });
        }
        
        if (response.ok) {
            alert(`Category ${isEdit ? 'updated' : 'created'} successfully!`);
            showCategoriesManagement();
        } else {
            const errorData = await response.json();
            alert(errorData.message || 'Failed to save category');
        }
    } catch (error) {
        console.error('Error saving category:', error);
        alert('An error occurred while saving the category');
    }
}

async function editCategory(categoryId) {
    await showAddCategoryForm(categoryId);
}

async function deleteCategory(categoryId) {
    if (!confirm('Are you sure you want to delete this category?')) return;
    
    try {
        const headers = getAuthHeaders();
        const response = await fetch(`${API_BASE_URL}/categories/${categoryId}`, {
            method: 'DELETE',
            headers
        });
        
        if (response.ok) {
            alert('Category deleted successfully!');
            showCategoriesManagement();
        } else {
            const errorData = await response.json();
            alert(errorData.message || 'Failed to delete category');
        }
    } catch (error) {
        console.error('Error deleting category:', error);
        alert('An error occurred while deleting the category');
    }
}



///

// User Management Functions
async function showUsersManagement() {
    hideAllSections();
    document.getElementById('usersManagement').style.display = 'block';
    
    try {
        const headers = getAuthHeaders();
        const response = await fetch(`${API_BASE_URL}/User`, { headers });
        const users = await response.json();
        
        const usersTable = document.getElementById('usersTable').querySelector('tbody');
        usersTable.innerHTML = '';
        
        for (const user of users) {
            const roles = await getUserRoles(user.id);
            
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${user.id}</td>
                <td>${user.email}</td>
                <td>${user.firstName} ${user.lastName}</td>
                <td>${user.phoneNumber || '-'}</td>
                <td>${user.address || '-'}</td>
                <td>${roles.join(', ')}</td>
                <td>
                    <span class="badge ${user.isActive ? 'bg-success' : 'bg-secondary'}">
                        ${user.isActive ? 'Active' : 'Inactive'}
                    </span>
                </td>
                <td>
                    <button class="btn btn-sm btn-primary edit-user" data-id="${user.id}">
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm btn-danger ${user.isActive ? 'deactivate-user' : 'activate-user'}" 
                            data-id="${user.id}" data-status="${user.isActive}">
                        <i class="bi ${user.isActive ? 'bi-person-x' : 'bi-person-check'}"></i>
                    </button>
                    ${user.isActive ? '' : `
                    <button class="btn btn-sm btn-danger delete-user" data-id="${user.id}">
                        <i class="bi bi-trash"></i>
                    </button>
                    `}
                </td>
            `;
            usersTable.appendChild(row);
        }
        
        // Add event listeners to action buttons
        document.querySelectorAll('.edit-user').forEach(btn => {
            btn.addEventListener('click', () => editUser(btn.dataset.id));
        });
        
        document.querySelectorAll('.deactivate-user, .activate-user').forEach(btn => {
            btn.addEventListener('click', () => toggleUserStatus(btn.dataset.id, btn.dataset.status === 'true'));
        });
        
        document.querySelectorAll('.delete-user').forEach(btn => {
            btn.addEventListener('click', () => deleteUser(btn.dataset.id));
        });
    } catch (error) {
        console.error('Error loading users:', error);
        alert('Failed to load users');
    }
}

async function getUserRoles(userId) {
    try {
        const headers = getAuthHeaders();
        const response = await fetch(`${API_BASE_URL}/User/${userId}/roles`, { headers });
        return await response.json();
    } catch (error) {
        console.error('Error getting user roles:', error);
        return ['Error loading roles'];
    }
}



async function getUserRoles(userId) {
    try {
        const headers = getAuthHeaders();
        const response = await fetch(`${API_BASE_URL}/User/${userId}/roles`, { headers });
        
        if (!response.ok) {
            if (response.status === 404) {
                return ['No roles found']; // Default value if endpoint not found
            }
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        return await response.json();
    } catch (error) {
        console.error('Error getting user roles:', error);
        return ['Error loading roles'];
    }
}

async function deleteUser(userId) {
    if (!confirm('Are you sure you want to permanently delete this user? This action cannot be undone.')) return;
    
    try {
        const headers = getAuthHeaders();
        const response = await fetch(`${API_BASE_URL}/User/${userId}`, {
            method: 'DELETE',
            headers
        });
        
        if (response.ok) {
            alert('User deleted successfully!');
            showUsersManagement();
        } else {
            const errorData = await response.json();
            alert(errorData.message || 'Failed to delete user');
        }
    } catch (error) {
        console.error('Error deleting user:', error);
        alert('An error occurred while deleting the user');
    }
}

// Update hideAllSections() to include the new sections
function hideAllSections() {
    document.getElementById('dashboardContent').style.display = 'none';
    document.getElementById('productsManagement').style.display = 'none';
    document.getElementById('productFormContainer').style.display = 'none';
    document.getElementById('categoriesManagement').style.display = 'none';
    document.getElementById('categoryFormContainer').style.display = 'none';
    document.getElementById('usersManagement').style.display = 'none';
    document.getElementById('userFormContainer').style.display = 'none';
}
//
// Helper functions


function getAuthHeaders() {
    const token = localStorage.getItem('authToken');
    if (!token) {
        console.error('No auth token found');
        // Handle this case appropriately (e.g., redirect to login)
    }
    return {
        'Authorization': `Bearer ${token}`,
        'Accept': 'application/json'
    };
}

function logout() {
    localStorage.removeItem('authToken');
    localStorage.removeItem('userEmail');
    window.location.href = 'login.html';
}