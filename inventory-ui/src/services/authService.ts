import api from './api';
import { ApiResponse, LoginDto, RegisterDto, AuthResponseDto, ChangePasswordDto, UserDto } from '../types/auth';

export class AuthService {
  async login(loginData: LoginDto): Promise<ApiResponse<AuthResponseDto>> {
    const response = await api.post('/auth/login', loginData);
    
    if (response.data.success) {
      localStorage.setItem('accessToken', response.data.data.accessToken);
      localStorage.setItem('user', JSON.stringify(response.data.data.user));
    }
    
    return response.data;
  }

  async register(registerData: RegisterDto): Promise<ApiResponse<UserDto>> {
    const response = await api.post('/auth/register', registerData);
    return response.data;
  }

  async logout(): Promise<void> {
    try {
      await api.post('/auth/logout');
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      localStorage.removeItem('accessToken');
      localStorage.removeItem('user');
    }
  }

  async getCurrentUser(): Promise<ApiResponse<UserDto>> {
    const response = await api.get('/auth/me');
    return response.data;
  }

  async changePassword(data: ChangePasswordDto): Promise<ApiResponse<string>> {
    const response = await api.post('/auth/change-password', data);
    return response.data;
  }

  getCurrentUserFromStorage(): UserDto | null {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  }

  isAuthenticated(): boolean {
    return !!localStorage.getItem('accessToken');
  }
}

export const authService = new AuthService(); 