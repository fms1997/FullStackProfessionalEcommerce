import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { env } from "../config/env";
import type { PagedResponse, ProductDto, ProductListQuery } from "../types/api";

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

export const catalogApi = createApi({
  reducerPath: "catalogApi",
  baseQuery: fetchBaseQuery({ baseUrl: env.API_BASE_URL }),
  tagTypes: ["Products"],
  endpoints: (builder) => ({
    getProducts: builder.query<PagedResponse<ProductDto>, ProductListQuery>({
      query: (q) => `/api/v1/products?${toQueryString(q)}`,
      providesTags: ["Products"],
    }),
    deleteProduct: builder.mutation<void, string>({
      query: (id) => ({
        url: `/api/v1/products/${id}`,
        method: "DELETE",
      }),
      invalidatesTags: ["Products"],
    }),
  }),
});

export const { useGetProductsQuery, useDeleteProductMutation } = catalogApi;
