export interface ProductDto {
  id: string;
  name: string;
  description: string;
  category: string;
  price: number;
  currentStock: number;
  minimumStock: number;
  maximumStock: number;
  isLowStock: boolean;
  isOverStock: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateProductDto {
  name: string;
  description: string;
  category: string;
  price: number;
  minimumStock: number;
  maximumStock: number;
  initialStock: number;
}

export interface UpdateProductDto {
  name: string;
  description: string;
  price: number;
}

export interface AddStockDto {
  quantity: number;
  reason: string;
}

export interface RemoveStockDto {
  quantity: number;
  reason: string;
}