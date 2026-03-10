import { createSlice, type PayloadAction } from "@reduxjs/toolkit";
import type { CartDto } from "../types/api";

type LocalCartItem = {
  productId: string;
  name: string;
  sku: string;
  unitPrice: number;
  quantity: number;
};

type CartState = {
  localItems: LocalCartItem[];
  serverCart: CartDto | null;
};

const storageKey = "fulspectrum_anonymous_cart";

const loadLocal = (): LocalCartItem[] => {
  try {
    const raw = localStorage.getItem(storageKey);
    if (!raw) return [];
    return JSON.parse(raw) as LocalCartItem[];
  } catch {
    return [];
  }
};

const persistLocal = (items: LocalCartItem[]) => {
  localStorage.setItem(storageKey, JSON.stringify(items));
};

const initialState: CartState = {
  localItems: loadLocal(),
  serverCart: null,
};

const cartSlice = createSlice({
  name: "cart",
  initialState,
  reducers: {
    hydrateServerCart(state, action: PayloadAction<CartDto>) {
      state.serverCart = action.payload;
    },
    clearLocalCart(state) {
      state.localItems = [];
      persistLocal(state.localItems);
    },
    addLocalItem(state, action: PayloadAction<Omit<LocalCartItem, "quantity"> & { quantity?: number }>) {
      const qty = action.payload.quantity ?? 1;
      const existing = state.localItems.find((x) => x.productId === action.payload.productId);
      if (existing) {
        existing.quantity += qty;
      } else {
        state.localItems.push({ ...action.payload, quantity: qty });
      }
      persistLocal(state.localItems);
    },
    updateLocalItem(state, action: PayloadAction<{ productId: string; quantity: number }>) {
      state.localItems = state.localItems
        .map((x) => (x.productId === action.payload.productId ? { ...x, quantity: action.payload.quantity } : x))
        .filter((x) => x.quantity > 0);
      persistLocal(state.localItems);
    },
    removeLocalItem(state, action: PayloadAction<string>) {
      state.localItems = state.localItems.filter((x) => x.productId !== action.payload);
      persistLocal(state.localItems);
    },
  },
});

export const { hydrateServerCart, clearLocalCart, addLocalItem, updateLocalItem, removeLocalItem } = cartSlice.actions;
export default cartSlice.reducer;
