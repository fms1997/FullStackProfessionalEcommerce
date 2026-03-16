import { useEffect, useMemo, useReducer, useRef } from "react";
import { Link } from "react-router-dom";
import { useAppSelector } from "../state/hooks";
 import { usePlaceOrderMutation, usePreviewCheckoutMutation } from "../state/api";
import type { ShippingAddress } from "../types/api";

type Step = "shipping" | "review" | "result";

type CheckoutState = {
  step: Step;
  address: ShippingAddress;
  errors: Partial<Record<keyof ShippingAddress, string>>;
};

type Action =
  | { type: "FIELD"; field: keyof ShippingAddress; value: string }
  | { type: "SET_ERRORS"; errors: CheckoutState["errors"] }
  | { type: "NEXT" }
  | { type: "BACK" }
  | { type: "RESET" };

const initialAddress: ShippingAddress = {
  fullName: "",
  addressLine1: "",
  addressLine2: "",
  city: "",
  state: "",
  postalCode: "",
  countryCode: "US",
};

const initialState: CheckoutState = {
  step: "shipping",
  address: initialAddress,
  errors: {},
};

function reducer(state: CheckoutState, action: Action): CheckoutState {
  switch (action.type) {
    case "FIELD":
      return {
        ...state,
        address: { ...state.address, [action.field]: action.value },
        errors: { ...state.errors, [action.field]: undefined },
      };
    case "SET_ERRORS":
      return { ...state, errors: action.errors };
    case "NEXT":
      return { ...state, step: state.step === "shipping" ? "review" : "result" };
    case "BACK":
      return { ...state, step: "shipping" };
    case "RESET":
      return initialState;
    default:
      return state;
  }
}

function validateAddress(address: ShippingAddress) {
  const required: Array<keyof ShippingAddress> = ["fullName", "addressLine1", "city", "state", "postalCode", "countryCode"];
  const errors: Partial<Record<keyof ShippingAddress, string>> = {};

  for (const field of required) {
    if (!address[field]?.trim()) {
      errors[field] = "Campo requerido";
    }
  }

  if (address.countryCode.trim().length !== 2) {
    errors.countryCode = "Usa un código ISO de 2 letras";
  }

  return errors;
}

export default function Checkout() {
  const [state, dispatch] = useReducer(reducer, initialState);
  const [previewCheckout, previewResult] = usePreviewCheckoutMutation();
  const [placeOrder, placeOrderResult] = usePlaceOrderMutation();
   const { serverCart } = useAppSelector((s) => s.cart);
  const titleRef = useRef<HTMLHeadingElement>(null);

  useEffect(() => {
    titleRef.current?.focus();
  }, [state.step]);

  const canCheckout = (serverCart?.items.length ?? 0) > 0;

  const onShippingNext = async () => {
    const errors = validateAddress(state.address);
    if (Object.keys(errors).length > 0) {
      dispatch({ type: "SET_ERRORS", errors });
      return;
    }

    const res = await previewCheckout({ shippingAddress: state.address });
    if ("data" in res) {
      dispatch({ type: "NEXT" });
    }
  };

  const onPlaceOrder = async () => {
    const orderRes = await placeOrder({ shippingAddress: state.address });
    if ("data" in orderRes && orderRes.data) {
       dispatch({ type: "NEXT" });
    }
  };

  const activeOrder = placeOrderResult.data;
  const progressLabel = useMemo(() => {
    if (state.step === "shipping") return "Paso 1 de 3: dirección";
    if (state.step === "review") return "Paso 2 de 3: revisión";
    return "Paso 3 de 3: confirmación";
  }, [state.step]);

  if (!canCheckout) {
    return (
      <section className="p-6 space-y-3">
        <h1 className="text-2xl font-semibold">Checkout</h1>
        <p>Tu carrito está vacío. Agrega productos para continuar.</p>
        <Link className="underline" to="/">
          Volver al catálogo
        </Link>
      </section>
    );
  }

  return (
    <section className="mx-auto max-w-3xl p-6 space-y-4">
      <h1 ref={titleRef} tabIndex={-1} className="text-2xl font-semibold focus:outline-none">
        Checkout profesional
      </h1>
      <p aria-live="polite" className="text-sm text-gray-600">
        {progressLabel}
      </p>

      {state.step === "shipping" && (
        <div className="space-y-3 border rounded p-4">
          <h2 className="font-medium">Dirección de envío</h2>
          {(Object.keys(state.errors).length > 0) && (
            <div role="alert" className="rounded border border-red-300 bg-red-50 p-2 text-sm">
              Revisa los campos marcados.
            </div>
          )}
          {(
            [
              ["fullName", "Nombre completo"],
              ["addressLine1", "Dirección"],
              ["addressLine2", "Departamento (opcional)"],
              ["city", "Ciudad"],
              ["state", "Provincia/Estado"],
              ["postalCode", "Código postal"],
              ["countryCode", "País (ISO)"]
            ] as Array<[keyof ShippingAddress, string]>
          ).map(([field, label]) => (
            <label key={field} className="block text-sm">
              {label}
              <input
                value={state.address[field] ?? ""}
                onChange={(e) => dispatch({ type: "FIELD", field, value: e.target.value })}
                aria-invalid={Boolean(state.errors[field])}
                className="mt-1 w-full border rounded px-2 py-1"
              />
              {state.errors[field] && <span className="text-red-600">{state.errors[field]}</span>}
            </label>
          ))}

          <button onClick={onShippingNext} disabled={previewResult.isLoading} className="rounded bg-black text-white px-4 py-2">
            {previewResult.isLoading ? "Calculando..." : "Continuar a revisión"}
          </button>
        </div>
      )}

      {state.step === "review" && previewResult.data && (
        <div className="space-y-3 border rounded p-4">
          <h2 className="font-medium">Revisión de orden</h2>
          <ul className="space-y-2">
            {previewResult.data.items.map((item) => (
              <li key={item.productId} className="flex justify-between text-sm">
                <span>{item.productName} x {item.quantity}</span>
                <strong>${item.lineTotal.toFixed(2)}</strong>
              </li>
            ))}
          </ul>
          <div className="text-sm space-y-1 border-t pt-2">
            <p>Subtotal: ${previewResult.data.totals.subtotal.toFixed(2)}</p>
            <p>Envío: ${previewResult.data.totals.shippingAmount.toFixed(2)}</p>
            <p>Impuestos: ${previewResult.data.totals.taxAmount.toFixed(2)}</p>
            <p className="font-semibold">Total: ${previewResult.data.totals.total.toFixed(2)}</p>
          </div>
          <div className="flex gap-2">
            <button onClick={() => dispatch({ type: "BACK" })} className="rounded border px-4 py-2">Editar dirección</button>
            <button onClick={onPlaceOrder} disabled={placeOrderResult.isLoading} className="rounded bg-black text-white px-4 py-2">
              Confirmar compra
            </button>
          </div>
        </div>
      )}

      {state.step === "result" && activeOrder && (
        <div className="space-y-3 border rounded p-4" role="status" aria-live="polite">
          <h2 className="font-medium">Orden creada</h2>
          <p>ID: {activeOrder.id}</p>
          <p>Estado actual: <strong>{activeOrder.status}</strong></p>
          <p>Total pagado: ${activeOrder.total.toFixed(2)}</p>
          <button onClick={() => dispatch({ type: "RESET" })} className="underline">Crear otra orden</button>
        </div>
      )}
    </section>
  );
}
