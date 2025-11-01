// types/cashRegister.ts

export interface CashRegister {
  id: string;
  activityId: string;
  activityName?: string;
  registerNumber: number;
  name: string;
  location: string;
  isOpen: boolean;
  openedAt?: string;
  closedAt?: string;
  operatorUserId?: string;
  operatorUserName?: string;
  supervisorUserId?: string;
  supervisorUserName?: string;
  createdAt: string;
}

export interface CreateCashRegisterRequest {
  activityId: string;
  registerNumber: number;
  name: string;
  location: string;
  operatorUserId?: string;
  supervisorUserId?: string;
}

export interface CloseCashRegisterRequest {
  cashDeclared: number;
  closedBy: string;
  supervisedBy?: string;
  observations?: string;
}

export interface CashRegisterClosure {
  id: string;
  cashRegisterId: string;
  openingDate: string;
  closingDate: string;
  totalTransactions: number;
  totalItemsSold: number;
  totalSalesAmount: number;
  cashCalculated: number;
  cardsCalculated: number;
  sinpeCalculated: number;
  cashDeclared: number;
  cashDifference: number;
  closedBy: string;
  closedByName?: string;
  supervisedBy?: string;
  supervisedByName?: string;
  observations?: string;
}