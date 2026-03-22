import { createApi, fetchBaseQuery, type BaseQueryFn, type FetchArgs, type FetchBaseQueryError } from "@reduxjs/toolkit/query/react";

import { env } from "../config/env";
import type {
  AdminOrderDto,
  AdminProductDto,
  CartDto,
  CheckoutPreview,
  OrderDto,
  OrderSummaryDto,
  OrderTrackingDto,
  PagedResponse,
  ProductDto,
  ProductListQuery,
  ShippingAddress,
} from "../types/api";
import { clearAuth, setCredentials, setForbidden, type UserProfile } from "./authSlice";
import type { RootState } from "./store";

type AuthResponse = {
  accessToken: string;
  expiresAtUtc: string;
  profile: UserProfile;
};

type CsrfResponse = {
  csrfToken: string;
};

let csrfToken: string | null = null;

const toQueryString = (query: ProductListQuery) => {
  const params = new URLSearchParams();

  if (query.search) {
    params.set("search", query.search);
  }

  if (typeof query.isPublished === "boolean") {
    params.set("isPublished", String(query.isPublished));
  }

  if (query.sortBy) {
    params.set("sortBy", query.sortBy);
  }

  if (query.sortDirection) {
    params.set("sortDirection", query.sortDirection);
  }

  if (query.page) {
    params.set("page", String(query.page));
  }

  if (query.pageSize) {
    params.set("pageSize", String(query.pageSize));
  }

  return params.toString();
};

const isUnsafeMethod = (method?: string) => {
  const normalizedMethod = (method ?? "GET").toUpperCase();
  return ["POST", "PUT", "PATCH", "DELETE"].includes(normalizedMethod);
};

const rawBaseQuery = fetchBaseQuery({
  baseUrl: env.API_BASE_URL,
  credentials: "include",
  prepareHeaders: (headers, { getState }) => {
    const token = (getState() as RootState).auth.accessToken;

    if (token) {
      headers.set("authorization", `Bearer ${token}`);
    }

    if (csrfToken) {
      headers.set("X-CSRF-TOKEN", csrfToken);
    }

    return headers;
  },
});

const ensureCsrfToken = async (
  api: Parameters<BaseQueryFn>[1],
  extraOptions: Parameters<BaseQueryFn>[2],
) => {
  if (csrfToken) {
    return;
  }

  const csrfResult = await rawBaseQuery(
    {
      url: "/api/v1/auth/csrf-token",
      method: "GET",
    },
    api,
    extraOptions,
  );

  if (csrfResult.data) {
    csrfToken = (csrfResult.data as CsrfResponse).csrfToken;
  }
};

const baseQueryWithReauth: BaseQueryFn<string | FetchArgs, unknown, FetchBaseQueryError> = async (
  args,
  api,
  extraOptions,
) => {
  const request: FetchArgs =
    typeof args === "string"
      ? { url: args, method: "GET" }
      : args;

  if (isUnsafeMethod(request.method)) {
    await ensureCsrfToken(api, extraOptions);
  }

  let result = await rawBaseQuery(args, api, extraOptions);

  if (result.error?.status === 401) {
    if (isUnsafeMethod("POST")) {
      await ensureCsrfToken(api, extraOptions);
    }

    const refreshResult = await rawBaseQuery(
      {
        url: "/api/v1/auth/refresh",
        method: "POST",
      },
      api,
      extraOptions,
    );

    if (refreshResult.data) {
      const auth = refreshResult.data as AuthResponse;

      api.dispatch(
        setCredentials({
          accessToken: auth.accessToken,
          profile: auth.profile,
        }),
      );

      result = await rawBaseQuery(args, api, extraOptions);
    } else {
      api.dispatch(clearAuth());
    }
  }

  if (result.error?.status === 403) {
    api.dispatch(setForbidden("No tienes permisos para realizar esta acción."));
  }

  return result;
};

export const catalogApi = createApi({
  reducerPath: "catalogApi",
  baseQuery: baseQueryWithReauth,
  tagTypes: ["Products", "Auth", "Cart", "Orders", "AdminProducts", "AdminOrders"],
  endpoints: (builder) => ({
    login: builder.mutation<AuthResponse, { email: string; password: string }>({
      query: (body) => ({
        url: "/api/v1/auth/login",
        method: "POST",
        body,
      }),
      invalidatesTags: ["Auth"],
    }),

    register: builder.mutation<
      AuthResponse,
      { email: string; password: string; firstName: string; lastName: string }
    >({
      query: (body) => ({
        url: "/api/v1/auth/register",
        method: "POST",
        body,
      }),
      invalidatesTags: ["Auth"],
    }),

    forgotPassword: builder.mutation<{ message: string; resetToken?: string }, { email: string }>({
      query: (body) => ({
        url: "/api/v1/auth/forgot-password",
        method: "POST",
        body,
      }),
    }),

    resetPassword: builder.mutation<{ message: string }, { token: string; newPassword: string }>({
      query: (body) => ({
        url: "/api/v1/auth/reset-password",
        method: "POST",
        body,
      }),
    }),

    getMe: builder.query<UserProfile, void>({
      query: () => "/api/v1/auth/me",
      providesTags: ["Auth"],
    }),

    logout: builder.mutation<void, void>({
      query: () => ({
        url: "/api/v1/auth/logout",
        method: "POST",
      }),
      invalidatesTags: ["Auth"],
    }),

    getAdminProducts: builder.query<AdminProductDto[], { search?: string; isPublished?: boolean }>({
      query: ({ search, isPublished }) => {
        const params = new URLSearchParams();

        if (search) {
          params.set("search", search);
        }

        if (typeof isPublished === "boolean") {
          params.set("isPublished", String(isPublished));
        }

        return `/api/v1/admin/catalog/products?${params.toString()}`;
      },
      providesTags: ["AdminProducts"],
    }),

    createAdminProduct: builder.mutation<AdminProductDto, unknown>({
      query: (body) => ({
        url: "/api/v1/admin/catalog/products",
        method: "POST",
        body,
      }),
      invalidatesTags: ["AdminProducts", "Products"],
    }),

    updateAdminProduct: builder.mutation<AdminProductDto, { id: string; body: unknown }>({
      query: ({ id, body }) => ({
        url: `/api/v1/admin/catalog/products/${id}`,
        method: "PUT",
        body,
      }),
      invalidatesTags: ["AdminProducts", "Products"],
    }),

    bulkPublishProducts: builder.mutation<
      { affected: number },
      { productIds: string[]; isPublished: boolean }
    >({
      query: (body) => ({
        url: "/api/v1/admin/catalog/products/bulk-publish",
        method: "POST",
        body,
      }),
      invalidatesTags: ["AdminProducts", "Products"],
    }),

    bulkDeleteProducts: builder.mutation<{ affected: number }, { productIds: string[] }>({
      query: (body) => ({
        url: "/api/v1/admin/catalog/products/bulk-delete",
        method: "DELETE",
        body,
      }),
      invalidatesTags: ["AdminProducts", "Products"],
    }),

    updateVariantStock: builder.mutation<
      unknown,
      {
        variantId: string;
        quantityOnHand: number;
        reservedQuantity: number;
        reorderThreshold: number;
      }
    >({
      query: ({ variantId, ...body }) => ({
        url: `/api/v1/admin/catalog/variants/${variantId}/stock`,
        method: "PATCH",
        body,
      }),
      invalidatesTags: ["AdminProducts"],
    }),

    uploadAdminImage: builder.mutation<{ url: string }, File>({
      query: (file) => {
        const formData = new FormData();
        formData.append("file", file);

        return {
          url: "/api/v1/admin/uploads/images",
          method: "POST",
          body: formData,
        };
      },
    }),

    getAdminOrders: builder.query<AdminOrderDto[], { status?: string; search?: string }>({
      query: ({ status, search }) => {
        const params = new URLSearchParams();

        if (status) {
          params.set("status", status);
        }

        if (search) {
          params.set("search", search);
        }

        return `/api/v1/admin/orders?${params.toString()}`;
      },
      providesTags: ["AdminOrders"],
    }),

    updateAdminOrderStatus: builder.mutation<void, { id: string; status: string }>({
      query: ({ id, status }) => ({
        url: `/api/v1/admin/orders/${id}/status`,
        method: "PATCH",
        body: { status },
      }),
      invalidatesTags: ["AdminOrders", "Orders"],
    }),

    getProducts: builder.query<PagedResponse<ProductDto>, ProductListQuery>({
      query: (query) => `/api/v1/products?${toQueryString(query)}`,
      providesTags: ["Products"],
    }),

    deleteProduct: builder.mutation<void, string>({
      query: (id) => ({
        url: `/api/v1/products/${id}`,
        method: "DELETE",
      }),
      invalidatesTags: ["Products"],
    }),

    getCart: builder.query<CartDto, void>({
      query: () => "/api/v1/cart",
      providesTags: ["Cart"],
    }),

    addCartItem: builder.mutation<CartDto, { productId: string; quantity: number; rowVersion?: string }>({
      query: (body) => ({
        url: "/api/v1/cart/items",
        method: "POST",
        body,
      }),
      invalidatesTags: ["Cart"],
    }),

    updateCartItem: builder.mutation<CartDto, { productId: string; quantity: number; rowVersion?: string }>({
      query: ({ productId, ...body }) => ({
        url: `/api/v1/cart/items/${productId}`,
        method: "PUT",
        body,
      }),
      invalidatesTags: ["Cart"],
    }),

    removeCartItem: builder.mutation<CartDto, { productId: string; rowVersion?: string }>({
      query: ({ productId, rowVersion }) => ({
        url: `/api/v1/cart/items/${productId}?rowVersion=${rowVersion ?? ""}`,
        method: "DELETE",
      }),
      invalidatesTags: ["Cart"],
    }),

    mergeCart: builder.mutation<
      CartDto,
      { items: Array<{ productId: string; quantity: number }>; rowVersion?: string }
    >({
      query: (body) => ({
        url: "/api/v1/cart/merge",
        method: "POST",
        body,
      }),
      invalidatesTags: ["Cart"],
    }),

    previewCheckout: builder.mutation<CheckoutPreview, { shippingAddress: ShippingAddress }>({
      query: (body) => ({
        url: "/api/v1/checkout/preview",
        method: "POST",
        body,
      }),
    }),

    placeOrder: builder.mutation<OrderDto, { shippingAddress: ShippingAddress }>({
      query: (body) => ({
        url: "/api/v1/checkout/orders",
        method: "POST",
        body,
      }),
      invalidatesTags: ["Cart", "Orders"],
    }),

    getMyOrders: builder.query<OrderSummaryDto[], void>({
      query: () => "/api/v1/orders",
      providesTags: ["Orders"],
    }),

    getOrderDetail: builder.query<OrderDto, string>({
      query: (orderId) => `/api/v1/orders/${orderId}`,
      providesTags: ["Orders"],
    }),

    getOrderTracking: builder.query<OrderTrackingDto, string>({
      query: (orderId) => `/api/v1/orders/${orderId}/tracking`,
      providesTags: ["Orders"],
    }),

    updateOrderStatus: builder.mutation<OrderDto, { orderId: string; status: string }>({
      query: ({ orderId, status }) => ({
        url: `/api/v1/orders/${orderId}/status`,
        method: "PATCH",
        body: { status },
      }),
      invalidatesTags: ["Orders"],
    }),
  }),
});

export const {
  useLoginMutation,
  useRegisterMutation,
  useForgotPasswordMutation,
  useResetPasswordMutation,
  useGetMeQuery,
  useLogoutMutation,
  useGetProductsQuery,
  useDeleteProductMutation,
  useGetCartQuery,
  useAddCartItemMutation,
  useUpdateCartItemMutation,
  useRemoveCartItemMutation,
  useMergeCartMutation,
  usePreviewCheckoutMutation,
  usePlaceOrderMutation,
  useGetMyOrdersQuery,
  useGetOrderDetailQuery,
  useGetOrderTrackingQuery,
  useUpdateOrderStatusMutation,
  useGetAdminProductsQuery,
  useCreateAdminProductMutation,
  useUpdateAdminProductMutation,
  useBulkPublishProductsMutation,
  useBulkDeleteProductsMutation,
  useUpdateVariantStockMutation,
  useUploadAdminImageMutation,
  useGetAdminOrdersQuery,
  useUpdateAdminOrderStatusMutation,
} = catalogApi;