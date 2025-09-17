import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { 
  CubeIcon, 
  ExclamationTriangleIcon, 
  ChartBarIcon, 
  SparklesIcon,
  ArrowTrendingUpIcon,
  ArrowTrendingDownIcon
} from '@heroicons/react/24/outline';

interface DashboardStats {
  totalProducts: number;
  lowStockProducts: number;
  totalValue: number;
  categories: number;
}

interface RecentActivity {
  id: string;
  type: 'stock_in' | 'stock_out' | 'new_product' | 'ai_suggestion';
  productName: string;
  quantity?: number;
  timestamp: string;
  description: string;
}

const Dashboard: React.FC = () => {
  const { user } = useAuth();
  const [stats, setStats] = useState<DashboardStats>({
    totalProducts: 0,
    lowStockProducts: 0,
    totalValue: 0,
    categories: 0
  });
  const [recentActivity, setRecentActivity] = useState<RecentActivity[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    setIsLoading(true);
    try {
      // Simulate API calls - replace with real API calls
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      setStats({
        totalProducts: 156,
        lowStockProducts: 23,
        totalValue: 45670000,
        categories: 8
      });

      setRecentActivity([
        {
          id: '1',
          type: 'stock_out',
          productName: 'Laptop Dell Inspiron 15',
          quantity: 2,
          timestamp: '2024-01-15T10:30:00Z',
          description: 'Xuất kho 2 sản phẩm cho đơn hàng #1001'
        },
        {
          id: '2',
          type: 'ai_suggestion',
          productName: 'Chuột không dây Logitech',
          timestamp: '2024-01-15T09:15:00Z',
          description: 'AI gợi ý nhập thêm 50 sản phẩm'
        },
        {
          id: '3',
          type: 'stock_in',
          productName: 'Bàn phím cơ Gaming',
          quantity: 10,
          timestamp: '2024-01-15T08:45:00Z',
          description: 'Nhập kho 10 sản phẩm mới'
        },
        {
          id: '4',
          type: 'new_product',
          productName: 'Màn hình Samsung 24"',
          timestamp: '2024-01-14T16:20:00Z',
          description: 'Thêm sản phẩm mới vào hệ thống'
        }
      ]);
    } catch (error) {
      console.error('Failed to load dashboard data:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(amount);
  };

  const getActivityIcon = (type: RecentActivity['type']) => {
    switch (type) {
      case 'stock_in':
        return <ArrowTrendingUpIcon className="h-5 w-5 text-green-500" />;
      case 'stock_out':
        return <ArrowTrendingDownIcon className="h-5 w-5 text-red-500" />;
      case 'new_product':
        return <CubeIcon className="h-5 w-5 text-blue-500" />;
      case 'ai_suggestion':
        return <SparklesIcon className="h-5 w-5 text-purple-500" />;
      default:
        return <ChartBarIcon className="h-5 w-5 text-gray-500" />;
    }
  };

  const formatTimeAgo = (timestamp: string) => {
    const now = new Date();
    const time = new Date(timestamp);
    const diffInHours = Math.floor((now.getTime() - time.getTime()) / (1000 * 60 * 60));
    
    if (diffInHours < 1) return 'Vừa xong';
    if (diffInHours < 24) return `${diffInHours} giờ trước`;
    return `${Math.floor(diffInHours / 24)} ngày trước`;
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 p-6">
        <div className="animate-pulse">
          <div className="h-8 bg-gray-300 rounded w-64 mb-6"></div>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
            {[1, 2, 3, 4].map(i => (
              <div key={i} className="bg-white rounded-lg shadow p-6">
                <div className="h-12 bg-gray-300 rounded mb-4"></div>
                <div className="h-4 bg-gray-300 rounded w-24 mb-2"></div>
                <div className="h-6 bg-gray-300 rounded w-16"></div>
              </div>
            ))}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="py-6">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="mb-8">
            <h1 className="text-3xl font-bold text-gray-900">
              Xin chào, {user?.fullName || user?.username}!
            </h1>
            <p className="mt-2 text-gray-600">
              Tổng quan hệ thống quản lý kho ngày hôm nay
            </p>
          </div>

          {/* Stats Cards */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
            <div className="bg-white rounded-lg shadow-sm p-6">
              <div className="flex items-center">
                <div className="flex-shrink-0">
                  <CubeIcon className="h-8 w-8 text-blue-600" />
                </div>
                <div className="ml-4">
                  <p className="text-sm font-medium text-gray-600">Tổng sản phẩm</p>
                  <p className="text-2xl font-bold text-gray-900">{stats.totalProducts}</p>
                </div>
              </div>
            </div>

            <div className="bg-white rounded-lg shadow-sm p-6">
              <div className="flex items-center">
                <div className="flex-shrink-0">
                  <ExclamationTriangleIcon className="h-8 w-8 text-red-600" />
                </div>
                <div className="ml-4">
                  <p className="text-sm font-medium text-gray-600">Tồn kho thấp</p>
                  <p className="text-2xl font-bold text-gray-900">{stats.lowStockProducts}</p>
                </div>
              </div>
            </div>

            <div className="bg-white rounded-lg shadow-sm p-6">
              <div className="flex items-center">
                <div className="flex-shrink-0">
                  <ChartBarIcon className="h-8 w-8 text-green-600" />
                </div>
                <div className="ml-4">
                  <p className="text-sm font-medium text-gray-600">Tổng giá trị</p>
                  <p className="text-2xl font-bold text-gray-900">
                    {formatCurrency(stats.totalValue)}
                  </p>
                </div>
              </div>
            </div>

            <div className="bg-white rounded-lg shadow-sm p-6">
              <div className="flex items-center">
                <div className="flex-shrink-0">
                  <SparklesIcon className="h-8 w-8 text-purple-600" />
                </div>
                <div className="ml-4">
                  <p className="text-sm font-medium text-gray-600">Danh mục</p>
                  <p className="text-2xl font-bold text-gray-900">{stats.categories}</p>
                </div>
              </div>
            </div>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Quick Actions */}
            <div className="lg:col-span-1">
              <div className="bg-white rounded-lg shadow-sm p-6">
                <h3 className="text-lg font-semibold text-gray-900 mb-4">
                  Thao tác nhanh
                </h3>
                <div className="space-y-3">
                                    <Link
                    to="/products/new"
                    className="block w-full text-left px-4 py-3 bg-blue-50 hover:bg-blue-100 rounded-lg transition-colors"
                  >
                    <div className="flex items-center">
                      <CubeIcon className="h-5 w-5 text-blue-600 mr-3" />
                      <span className="text-blue-600 font-medium">Thêm sản phẩm mới</span>
                    </div>
                  </Link>

                  <Link
                    to="/products?filter=low-stock"
                    className="block w-full text-left px-4 py-3 bg-red-50 hover:bg-red-100 rounded-lg transition-colors"
                  >
                    <div className="flex items-center">
                      <ExclamationTriangleIcon className="h-5 w-5 text-red-600 mr-3" />
                      <span className="text-red-600 font-medium">Xem tồn kho thấp</span>
                    </div>
                  </Link>

                  <Link
                    to="/ai/stock-recommendations"
                    className="block w-full text-left px-4 py-3 bg-purple-50 hover:bg-purple-100 rounded-lg transition-colors"
                  >
                    <div className="flex items-center">
                      <SparklesIcon className="h-5 w-5 text-purple-600 mr-3" />
                      <span className="text-purple-600 font-medium">Gợi ý AI nhập hàng</span>
                    </div>
                  </Link>

                  <Link
                    to="/inventory/transactions"
                    className="block w-full text-left px-4 py-3 bg-green-50 hover:bg-green-100 rounded-lg transition-colors"
                  >
                    <div className="flex items-center">
                      <ChartBarIcon className="h-5 w-5 text-green-600 mr-3" />
                      <span className="text-green-600 font-medium">Xem báo cáo</span>
                    </div>
                  </Link>
                </div>
              </div>
            </div>

            {/* Recent Activity */}
            <div className="lg:col-span-2">
              <div className="bg-white rounded-lg shadow-sm p-6">
                <div className="flex items-center justify-between mb-4">
                  <h3 className="text-lg font-semibold text-gray-900">
                    Hoạt động gần đây
                  </h3>
                  <Link
                    to="/inventory/transactions"
                    className="text-blue-600 hover:text-blue-500 text-sm font-medium"
                  >
                    Xem tất cả
                  </Link>
                </div>
                
                <div className="space-y-4">
                  {recentActivity.map((activity) => (
                    <div key={activity.id} className="flex items-start space-x-3">
                      <div className="flex-shrink-0">
                        {getActivityIcon(activity.type)}
                      </div>
                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-medium text-gray-900">
                          {activity.productName}
                        </p>
                        <p className="text-sm text-gray-500">
                          {activity.description}
                        </p>
                        <p className="text-xs text-gray-400 mt-1">
                          {formatTimeAgo(activity.timestamp)}
                        </p>
                      </div>
                      {activity.quantity && (
                        <div className="flex-shrink-0">
                          <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                            activity.type === 'stock_in' 
                              ? 'bg-green-100 text-green-800' 
                              : 'bg-red-100 text-red-800'
                          }`}>
                            {activity.type === 'stock_in' ? '+' : '-'}{activity.quantity}
                          </span>
                        </div>
                      )}
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>

          {/* Low Stock Alert */}
          {stats.lowStockProducts > 0 && (
            <div className="mt-6">
              <div className="bg-yellow-50 border-l-4 border-yellow-400 p-4">
                <div className="flex">
                  <div className="flex-shrink-0">
                    <ExclamationTriangleIcon className="h-5 w-5 text-yellow-400" />
                  </div>
                  <div className="ml-3">
                    <p className="text-sm text-yellow-700">
                      <strong>Cảnh báo:</strong> Có {stats.lowStockProducts} sản phẩm 
                      đang có tồn kho thấp. 
                      <Link 
                        to="/products?filter=low-stock" 
                        className="font-medium underline hover:text-yellow-600 ml-1"
                      >
                        Xem chi tiết
                      </Link>
                    </p>
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default Dashboard;