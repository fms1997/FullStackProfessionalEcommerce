import { env } from "../config/env";
import type { CartDto, PagedResponse, ProductDto, ProductListQuery } from "../types/api";
import {
  createApi,
  fetchBaseQuery,
  type BaseQueryFn,
  type FetchArgs,
  type FetchBaseQueryError,
} from "@reduxjs/toolkit/query/react";
import { clearAuth, setCredentials, setForbidden, type UserProfile } from "./authSlice";
import type { RootState } from "./store";
const toQueryString = (query: ProductListQuery) => {
  const params = new URLSearchParams();

  if (query.search) params.set("search", query.search);
  if (typeof query.isPublished === "boolean") params.set("isPublished", String(query.isPublished));
  if (query.sortBy) params.set("sortBy", query.sortBy);
  if (query.sortDirection) params.set("sortDirection", query.sortDirection);
  if (query.page) params.set("page", String(query.page));
  if (query.pageSize) params.set("pageSize", String(query.pageSize));

  return params.toString();
};
type AuthResponse = {
  accessToken: string;
  expiresAtUtc: string;
  profile: UserProfile;
};

const rawBaseQuery = fetchBaseQuery({
  baseUrl: env.API_BASE_URL,
  credentials: "include",
  prepareHeaders: (headers, { getState }) => {
    const token = (getState() as RootState).auth.accessToken;
    if (token) headers.set("authorization", `Bearer ${token}`);
    return headers;
  },
});

const baseQueryWithReauth: BaseQueryFn<string | FetchArgs, unknown, FetchBaseQueryError> = async (
  args,
  api,
  extraOptions,
) => {
  let result = await rawBaseQuery(args, api, extraOptions);

  if (result.error?.status === 401) {
    const refreshResult = await rawBaseQuery({ url: "/api/v1/auth/refresh", method: "POST" }, api, extraOptions);

    if (refreshResult.data) {
      const auth = refreshResult.data as AuthResponse;
      api.dispatch(setCredentials({ accessToken: auth.accessToken, profile: auth.profile }));
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
  tagTypes: ["Products", "Auth", "Cart"],
  endpoints: (builder) => ({
    login: builder.mutation<AuthResponse, { email: string; password: string }>({
      query: (body) => ({ url: "/api/v1/auth/login", method: "POST", body }),
      invalidatesTags: ["Auth"],
    }),
    register: builder.mutation<AuthResponse, { email: string; password: string; firstName: string; lastName: string }>({
      query: (body) => ({ url: "/api/v1/auth/register", method: "POST", body }),
      invalidatesTags: ["Auth"],
    }),
    forgotPassword: builder.mutation<{ message: string; resetToken?: string }, { email: string }>({
      query: (body) => ({ url: "/api/v1/auth/forgot-password", method: "POST", body }),
    }),
    resetPassword: builder.mutation<{ message: string }, { token: string; newPassword: string }>({
      query: (body) => ({ url: "/api/v1/auth/reset-password", method: "POST", body }),
    }),
    getMe: builder.query<UserProfile, void>({
      query: () => "/api/v1/auth/me",
      providesTags: ["Auth"],
    }),
    logout: builder.mutation<void, void>({
      query: () => ({ url: "/api/v1/auth/logout", method: "POST" }),
      invalidatesTags: ["Auth"],
    }),
    getProducts: builder.query<PagedResponse<ProductDto>, ProductListQuery>({
      query: (q) => `/api/v1/products?${toQueryString(q)}`,
      providesTags: ["Products"],
    }),
    deleteProduct: builder.mutation<void, string>({
      query: (id) => ({ url: `/api/v1/products/${id}`, method: "DELETE" }),
            invalidatesTags: ["Products"],
    }),
     getCart: builder.query<CartDto, void>({
      query: () => "/api/v1/cart",
      providesTags: ["Cart"],
    }),
    addCartItem: builder.mutation<CartDto, { productId: string; quantity: number; rowVersion?: string }>({
      query: (body) => ({ url: "/api/v1/cart/items", method: "POST", body }),
      invalidatesTags: ["Cart"],
    }),
    updateCartItem: builder.mutation<CartDto, { productId: string; quantity: number; rowVersion?: string }>({
      query: ({ productId, ...body }) => ({ url: `/api/v1/cart/items/${productId}`, method: "PUT", body }),
      invalidatesTags: ["Cart"],
    }),
    removeCartItem: builder.mutation<CartDto, { productId: string; rowVersion?: string }>({
      query: ({ productId, rowVersion }) => ({ url: `/api/v1/cart/items/${productId}?rowVersion=${rowVersion ?? ""}`, method: "DELETE" }),
      invalidatesTags: ["Cart"],
    }),
    mergeCart: builder.mutation<CartDto, { items: Array<{ productId: string; quantity: number }>; rowVersion?: string }>({
      query: (body) => ({ url: "/api/v1/cart/merge", method: "POST", body }),
      invalidatesTags: ["Cart"],
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
} = catalogApi;