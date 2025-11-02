export interface SalesCombo {
  id: string;
  activityId: string;
  activityName: string;
  name: string;
  description?: string;
  comboPrice: number;
  active: boolean;
  createdAt: string;
  items: ComboItem[];
}

export interface ComboItem {
  id: string;
  productId: string;
  productName: string;
  productPrice: number;
  quantity: number;
}

export interface CreateComboRequest {
  activityId: string;
  name: string;
  description?: string;
  comboPrice: number;
  items: CreateComboItem[];
}

export interface CreateComboItem {
  productId: string;
  quantity: number;
}