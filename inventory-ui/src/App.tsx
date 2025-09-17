import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import PrivateRoute from './components/auth/PrivateRoute';
import Layout from './components/layout/Layout';
import LoginPage from './pages/auth/LoginPage';
import Dashboard from './pages/dashboard/Dashboard';
import ProductsList from './pages/products/ProductsList';
import ProductForm from './pages/products/ProductForm';
import ProductDetail from './pages/products/ProductDetail';
import AIRecommendations from './pages/ai/AIRecommendations';
import CategorySuggestions from './pages/ai/CategorySuggestions';
import StockAnalysis from './pages/ai/StockAnalysis';
import InventoryTransactions from './pages/inventory/InventoryTransactions';
import UserProfile from './pages/profile/UserProfile';
import NotFound from './pages/NotFound';

function App() {
  return (
    <Router>
      <AuthProvider>
        <div className="App">
          <Routes>
            {/* Public Routes */}
            <Route path="/login" element={<LoginPage />} />
            
            {/* Protected Routes */}
            <Route path="/" element={<PrivateRoute><Layout /></PrivateRoute>}>
              <Route index element={<Navigate to="/dashboard" replace />} />
              <Route path="dashboard" element={<Dashboard />} />
              
              {/* Products Routes */}
              <Route path="products">
                <Route index element={<ProductsList />} />
                <Route path="new" element={<ProductForm />} />
                <Route path=":id" element={<ProductDetail />} />
                <Route path=":id/edit" element={<ProductForm />} />
              </Route>
              
              {/* AI Features Routes */}
              <Route path="ai">
                <Route path="stock-recommendations" element={<AIRecommendations />} />
                <Route path="category-suggestions" element={<CategorySuggestions />} />
                <Route path="stock-analysis/:productId" element={<StockAnalysis />} />
              </Route>
              
              {/* Inventory Routes */}
              <Route path="inventory">
                <Route path="transactions" element={<InventoryTransactions />} />
              </Route>
              
              {/* Profile Routes */}
              <Route path="profile" element={<UserProfile />} />
            </Route>
            
            {/* 404 Route */}
            <Route path="*" element={<NotFound />} />
          </Routes>
        </div>
      </AuthProvider>
    </Router>
  );
}

export default App;