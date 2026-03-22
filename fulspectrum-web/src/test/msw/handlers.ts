import { http, HttpResponse } from "msw";

export const handlers = [
  http.get("/api/v1/products", () =>
    HttpResponse.json([
      { id: "p1", name: "Producto test", unitPrice: 1999, stock: 10 },
    ]),
  ),

  http.post("/api/v1/checkout/preview", async () =>
    HttpResponse.json({
      items: [
        {
          productId: "p1",
          productName: "Producto test",
          quantity: 1,
          unitPrice: 1999,
          lineTotal: 1999,
        },
      ],
      totals: {
        subtotal: 1999,
        shippingAmount: 0,
        taxAmount: 0,
        total: 1999,
      },
    }),
  ),
];
