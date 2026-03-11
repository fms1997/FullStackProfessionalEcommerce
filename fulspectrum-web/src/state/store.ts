import { configureStore } from "@reduxjs/toolkit";
import { setupListeners } from "@reduxjs/toolkit/query";
import { catalogApi } from "./api";
import authReducer from "./authSlice";
import cartReducer from "./cartSlice";
export const store = configureStore({
  reducer: {
    auth: authReducer,
    [catalogApi.reducerPath]: catalogApi.reducer,
    cart: cartReducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(catalogApi.middleware),
});

setupListeners(store.dispatch);

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
