import React, { forwardRef, useState } from 'react';
import { Eye, EyeOff } from 'lucide-react';

export interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  icon?: React.ReactNode;
  headerAction?: React.ReactNode;
  isPassword?: boolean;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, icon, headerAction, isPassword, type = 'text', className = '', ...props }, ref) => {
    const [showPassword, setShowPassword] = useState(false);

    const inputType = isPassword ? (showPassword ? 'text' : 'password') : type;

    return (
      <div className="w-full">
        {(label || headerAction) && (
          <div className="flex justify-between items-center mb-1.5">
            {label && (
              <label className="text-sm font-medium text-slate-700">
                {label}
              </label>
            )}
            {headerAction && (
              <div>{headerAction}</div>
            )}
          </div>
        )}

        <div className="relative flex items-center">
          {icon && (
            <div className="absolute left-3.5 flex items-center pointer-events-none text-slate-400">
              {icon}
            </div>
          )}

          <input
            ref={ref}
            type={inputType}
            className={`w-full py-2.5 rounded-lg border text-sm text-slate-800 placeholder:text-slate-400 bg-white transition-all outline-none ${
              icon ? 'pl-10' : 'pl-3.5'
            } ${
              isPassword ? 'pr-10' : 'pr-3.5'
            } ${
              error
                ? 'border-red-400 focus:ring-2 focus:ring-red-200'
                : 'border-slate-200 hover:border-slate-300 focus:border-[#0d5c68] focus:ring-2 focus:ring-[#0d5c68]/15'
            } ${className}`}
            {...props}
          />

          {isPassword && (
            <button
              type="button"
              onClick={() => setShowPassword(!showPassword)}
              className="absolute right-3.5 text-slate-400 hover:text-slate-600 focus:outline-none transition-colors"
              tabIndex={-1}
              aria-label={showPassword ? 'Ocultar contraseña' : 'Ver contraseña'}
            >
              {showPassword ? (
                <EyeOff className="w-4 h-4" />
              ) : (
                <Eye className="w-4 h-4" />
              )}
            </button>
          )}
        </div>

        {error && (
          <p className="mt-1 text-xs text-red-500 font-medium">{error}</p>
        )}
      </div>
    );
  }
);

Input.displayName = 'Input';
