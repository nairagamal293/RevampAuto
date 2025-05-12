document.addEventListener('DOMContentLoaded', () => {
    // Load products when products section is shown
    document.getElementById('products-list').addEventListener('show', loadProducts);
    
    // Load categories for dropdowns
    loadCategoriesForDropdowns();
    
    // Set up product image preview
    const productImagesInput = document.getElementById('productImages');
    if (productImagesInput) {
        productImagesInput.addEventListener('change', previewImages);
    }
    
    // Add product form submission
    const addProductForm = document.getElementById('addProductForm');
    if (addProductForm) {
        addProductForm.addEventListener('submit', handleAddProduct);
    }
    
    // Edit product form submission
    const editProductForm = document.getElementById('editProductForm');
    if (editProductForm) {
        editProductForm.addEventListener('submit', handleEditProduct);
    }
});

// products.js
async function loadProducts() {
    try {
        const token = localStorage.getItem('token');
        const response = await fetch(`${API_CONFIG.BASE_URL}${API_CONFIG.ENDPOINTS.PRODUCTS}`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        
        if (!response.ok) {
            throw new Error('Failed to fetch products');
        }
        
        const products = await response.json();
        
        const productsTable = document.getElementById('productsTable');
        productsTable.innerHTML = '';
        
        products.forEach(product => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${product.Id}</td>
                <td>
                    ${product.MainImageUrl ? 
                        `<img src="${product.MainImageUrl}" alt="${product.Name}" style="width: 50px; height: 50px; object-fit: cover;">` : 
                        'No Image'}
                </td>
                <td>${product.Name}</td>
                <td>$${product.Price.toFixed(2)}</td>
                <td>${product.StockQuantity}</td>
                <td>${product.CategoryName}</td>
                <td>
                    <button class="btn btn-sm btn-primary me-1" onclick="editProduct(${product.Id})">
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="deleteProduct(${product.Id})">
                        <i class="bi bi-trash"></i>
                    </button>
                </td>
            `;
            productsTable.appendChild(row);
        });
    } catch (error) {
        console.error('Failed to load products:', error);
        alert(error.message);
    }
}

async function addProduct(productData) {
    try {
        const token = localStorage.getItem('token');
        const response = await fetch(`${API_CONFIG.BASE_URL}${API_CONFIG.ENDPOINTS.PRODUCTS}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(productData)
        });
        
        if (!response.ok) {
            throw new Error('Failed to add product');
        }
        
        return await response.json();
    } catch (error) {
        console.error('Error adding product:', error);
        throw error;
    }
}

async function loadCategoriesForDropdowns() {
    try {
        const token = localStorage.getItem('token');
        const categories = await makeRequest('/categories', 'GET', null, token);
        
        const productCategorySelect = document.getElementById('productCategory');
        const editProductCategorySelect = document.getElementById('editProductCategory');
        
        // Clear existing options except the first one
        while (productCategorySelect.options.length > 1) {
            productCategorySelect.remove(1);
        }
        
        while (editProductCategorySelect.options.length > 1) {
            editProductCategorySelect.remove(1);
        }
        
        // Add new options
        categories.forEach(category => {
            const option = document.createElement('option');
            option.value = category.Id;
            option.textContent = category.Name;
            
            productCategorySelect.appendChild(option.cloneNode(true));
            editProductCategorySelect.appendChild(option.cloneNode(true));
        });
    } catch (error) {
        console.error('Failed to load categories:', error);
    }
}

function previewImages() {
    const input = document.getElementById('productImages');
    const preview = document.getElementById('imagePreview');
    preview.innerHTML = '';
    
    if (input.files) {
        Array.from(input.files).forEach(file => {
            const reader = new FileReader();
            
            reader.onload = function(e) {
                const img = document.createElement('img');
                img.src = e.target.result;
                preview.appendChild(img);
            }
            
            reader.readAsDataURL(file);
        });
    }
}

async function handleAddProduct(e) {
    e.preventDefault();
    
    const formData = {
        Name: document.getElementById('productName').value,
        Description: document.getElementById('productDescription').value,
        Price: parseFloat(document.getElementById('productPrice').value),
        StockQuantity: parseInt(document.getElementById('productStock').value),
        CategoryId: parseInt(document.getElementById('productCategory').value)
    };
    
    const imagesInput = document.getElementById('productImages');
    const images = imagesInput.files;
    
    try {
        const token = localStorage.getItem('token');
        
        // First create the product
        const product = await makeRequest('/products', 'POST', formData, token);
        
        // Then upload images if any
        if (images.length > 0) {
            await uploadProductImages(product.Id, images, true);
        }
        
        alert('Product added successfully!');
        showSection('products-list');
        loadProducts();
    } catch (error) {
        console.error('Failed to add product:', error);
        alert(error.message);
    }
}

async function uploadProductImages(productId, files, setMain = false) {
    const token = localStorage.getItem('token');
    const formData = new FormData();
    
    Array.from(files).forEach(file => {
        formData.append('files', file);
    });
    
    try {
        const response = await fetch(`${API_BASE_URL}/products/${productId}/images?setMain=${setMain}`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`
            },
            body: formData
        });
        
        if (!response.ok) {
            throw new Error('Failed to upload images');
        }
        
        return await response.json();
    } catch (error) {
        console.error('Image upload failed:', error);
        throw error;
    }
}

async function editProduct(productId) {
    try {
        const token = localStorage.getItem('token');
        const product = await makeRequest(`/products/${productId}`, 'GET', null, token);
        
        // Populate the edit form
        document.getElementById('editProductId').value = product.Id;
        document.getElementById('editProductName').value = product.Name;
        document.getElementById('editProductDescription').value = product.Description;
        document.getElementById('editProductPrice').value = product.Price;
        document.getElementById('editProductStock').value = product.StockQuantity;
        document.getElementById('editProductCategory').value = product.CategoryId;
        
        // Load current images
        const currentImagesContainer = document.getElementById('currentImages');
        currentImagesContainer.innerHTML = '';
        
        product.ImageUrls.forEach(imageUrl => {
            const imgContainer = document.createElement('div');
            imgContainer.className = 'position-relative d-inline-block';
            imgContainer.innerHTML = `
                <img src="${imageUrl}" class="img-thumbnail me-2 mb-2" style="width: 100px; height: 100px; object-fit: cover;">
                <button class="btn btn-danger btn-sm position-absolute top-0 end-0" 
                        onclick="deleteProductImage(${productId}, '${imageUrl.split('/').pop()}')">
                    <i class="bi bi-trash"></i>
                </button>
            `;
            currentImagesContainer.appendChild(imgContainer);
        });
        
        // Show the modal
        const modal = new bootstrap.Modal(document.getElementById('editProductModal'));
        modal.show();
    } catch (error) {
        console.error('Failed to load product for editing:', error);
        alert(error.message);
    }
}

async function handleEditProduct(e) {
    e.preventDefault();
    
    const productId = document.getElementById('editProductId').value;
    const formData = {
        Name: document.getElementById('editProductName').value,
        Description: document.getElementById('editProductDescription').value,
        Price: parseFloat(document.getElementById('editProductPrice').value),
        StockQuantity: parseInt(document.getElementById('editProductStock').value),
        CategoryId: parseInt(document.getElementById('editProductCategory').value)
    };
    
    const newImagesInput = document.getElementById('newProductImages');
    const newImages = newImagesInput.files;
    
    try {
        const token = localStorage.getItem('token');
        
        // Update the product
        await makeRequest(`/products/${productId}`, 'PUT', formData, token);
        
        // Upload new images if any
        if (newImages.length > 0) {
            await uploadProductImages(productId, newImages);
        }
        
        alert('Product updated successfully!');
        loadProducts();
        
        // Hide the modal
        const modal = bootstrap.Modal.getInstance(document.getElementById('editProductModal'));
        modal.hide();
    } catch (error) {
        console.error('Failed to update product:', error);
        alert(error.message);
    }
}

async function deleteProduct(productId) {
    if (!confirm('Are you sure you want to delete this product?')) {
        return;
    }
    
    try {
        const token = localStorage.getItem('token');
        await makeRequest(`/products/${productId}`, 'DELETE', null, token);
        
        alert('Product deleted successfully!');
        loadProducts();
    } catch (error) {
        console.error('Failed to delete product:', error);
        alert(error.message);
    }
}

async function deleteProductImage(productId, imageName) {
    if (!confirm('Are you sure you want to delete this image?')) {
        return;
    }
    
    try {
        const token = localStorage.getItem('token');
        
        // First we need to get the image ID
        const product = await makeRequest(`/products/${productId}`, 'GET', null, token);
        const image = product.Images.find(img => img.ImagePath.includes(imageName));
        
        if (!image) {
            throw new Error('Image not found');
        }
        
        await makeRequest(`/products/${productId}/images/${image.Id}`, 'DELETE', null, token);
        
        // Reload the product for editing
        editProduct(productId);
    } catch (error) {
        console.error('Failed to delete product image:', error);
        alert(error.message);
    }
}

// Make functions available globally
window.editProduct = editProduct;
window.deleteProduct = deleteProduct;
window.deleteProductImage = deleteProductImage;