export interface SalesTransaction {
  id: string;
  cashRegisterId: string;
  transactionNumber: string;
  invoiceNumber?: string;
  salesStatusId: number;
  statusName?: string;
  transactionDate: string;
  totalAmount: number;
  details: TransactionDetail[];
  payments: TransactionPayment[];
}

export interface TransactionDetail {
  id: string;
  productId?: string;
  productName?: string;
  comboId?: string;
  comboName?: string;
  quantity: number;
  unitPrice: number;
  totalAmount: number;
  isCombo: boolean;
}

export interface TransactionPayment {
  id: string;
  paymentMethodId: number;
  paymentMethodName?: string;
  amount: number;
  reference?: string;
  processedAt: string;
  processedBy: string;
  processedByName?: string;
}

export interface CreateSaleRequest {
  cashRegisterId: string;
  items: CreateSaleItem[];
  createdBy: string;
}

export interface CreateSaleItem {
  productId: string;
  quantity: number;
}

export interface CreatePaymentRequest {
  paymentMethodId: number;
  amount: number;
  reference?: string;
  processedBy: string;
}

export interface CompleteSaleRequest {
  payments: CreatePaymentRequest[];
}

export interface SalesSummary {
  date: string;
  totalTransactions: number;
  completedTransactions: number;
  totalSales: number;
  averageTransaction: number;
  totalItemsSold: number;
}

export interface PaymentMethod {
  id: number;
  name: string;
  requiresReference: boolean;
  active: boolean;
}

export interface SalesStatus {
  id: number;
  name: string;
  description?: string;
  active: boolean;
}