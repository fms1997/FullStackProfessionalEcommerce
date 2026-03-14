// import { useState } from "react";
// import { useForgotPasswordMutation, useResetPasswordMutation } from "../../state/api";

// export default function ForgotPassword() {
//   const [email, setEmail] = useState("");
//   const [token, setToken] = useState("");
//   const [newPassword, setNewPassword] = useState("");
//   const [previewToken, setPreviewToken] = useState("");
//   const [forgotPassword, forgotState] = useForgotPasswordMutation();
//   const [resetPassword, resetState] = useResetPasswordMutation();

//   return (
//     <div className="max-w-md mx-auto mt-8 space-y-6">
//       <form
//         onSubmit={async (e) => {
//           e.preventDefault();
//           const result = await forgotPassword({ email });
//           if ("data" in result && result.data) setPreviewToken(result.data.resetToken ?? "");
//         }}
//         className="space-y-3 border rounded p-4"
//       >
//         <h1 className="text-lg font-semibold">Recuperar contraseña</h1>
//         <input className="w-full border rounded px-3 py-2" placeholder="Email" value={email} onChange={(e) => setEmail(e.target.value)} />
//         <button className="border rounded px-3 py-2" disabled={forgotState.isLoading}>Enviar instrucciones</button>
//         {previewToken && <p className="text-xs break-all">Token demo: {previewToken}</p>}
//       </form>

//       <form
//         onSubmit={async (e) => {
//           e.preventDefault();
//           await resetPassword({ token, newPassword });
//         }}
//         className="space-y-3 border rounded p-4"
//       >
//         <h2 className="text-lg font-semibold">Restablecer contraseña</h2>
//         <input className="w-full border rounded px-3 py-2" placeholder="Token" value={token} onChange={(e) => setToken(e.target.value)} />
//         <input className="w-full border rounded px-3 py-2" type="password" placeholder="Nueva contraseña" value={newPassword} onChange={(e) => setNewPassword(e.target.value)} />
//         <button className="border rounded px-3 py-2" disabled={resetState.isLoading}>Cambiar contraseña</button>
//       </form>
//     </div>
//   );
// }

import { useState } from "react";
import {
  useForgotPasswordMutation,
  useResetPasswordMutation,
} from "../../state/api";

export default function ForgotPassword() {
  const [email, setEmail] = useState("");
  const [token, setToken] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [previewToken, setPreviewToken] = useState("");

  const [forgotPassword, forgotState] = useForgotPasswordMutation();
  const [resetPassword, resetState] = useResetPasswordMutation();

  return (
    <div className="max-w-md mx-auto mt-8 space-y-6">
      <form
        onSubmit={async (e) => {
          e.preventDefault();

          const result = await forgotPassword({ email });

          if ("data" in result && result.data) {
            const generatedToken = result.data.resetToken ?? "";
            setPreviewToken(generatedToken);
            setToken(generatedToken); // autocompleta el form de reset
          }
        }}
        className="space-y-3 border rounded p-4"
      >
        <h1 className="text-lg font-semibold">Recuperar contraseña</h1>

        <input
          className="w-full border rounded px-3 py-2"
          placeholder="Email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
        />

        <button
          className="border rounded px-3 py-2"
          disabled={forgotState.isLoading}
          type="submit"
        >
          Enviar instrucciones
        </button>

        {previewToken && (
          <p className="text-xs break-all">Token demo: {previewToken}</p>
        )}
      </form>

      <form
        onSubmit={async (e) => {
          e.preventDefault();
          // await resetPassword({ email, token, newPassword });
          await resetPassword({ token, newPassword });
        }}
        className="space-y-3 border rounded p-4"
      >
        <h2 className="text-lg font-semibold">Restablecer contraseña</h2>

        <input
          className="w-full border rounded px-3 py-2"
          placeholder="Email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
        />

        <input
          className="w-full border rounded px-3 py-2"
          placeholder="Token"
          value={token}
          onChange={(e) => setToken(e.target.value)}
        />

        <input
          className="w-full border rounded px-3 py-2"
          type="password"
          placeholder="Nueva contraseña"
          value={newPassword}
          onChange={(e) => setNewPassword(e.target.value)}
        />

        <button
          className="border rounded px-3 py-2"
          disabled={resetState.isLoading}
          type="submit"
        >
          Cambiar contraseña
        </button>
      </form>
    </div>
  );
}
