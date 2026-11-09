import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { LoginPage } from '../pages/LoginPage';
import { AppLayout } from '../components/layout/AppLayout';
import { MedicoDashboardPage } from '../pages/medico/MedicoDashboardPage';
import { AgendaTurnosPage } from '../pages/medico/AgendaTurnosPage';
import { HorariosMedicoPage } from '../pages/medico/HorariosMedicoPage';
import { PrescripcionesPage } from '../pages/medico/PrescripcionesPage';

export const AppRoutes: React.FC = () => {
  return (
    <BrowserRouter>
      <Routes>
        {/* Ruta de Login */}
        <Route path="/login" element={<LoginPage />} />

        {/* Módulo del Profesional Médico */}
        <Route path="/medico" element={<AppLayout />}>
          <Route index element={<MedicoDashboardPage />} />
          <Route path="turnos" element={<AgendaTurnosPage />} />
          <Route path="horarios" element={<HorariosMedicoPage />} />
          <Route path="prescripciones" element={<PrescripcionesPage />} />
          <Route path="*" element={<Navigate to="/medico" replace />} />
        </Route>

        {/* Redirección por defecto */}
        <Route path="/" element={<Navigate to="/login" replace />} />
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    </BrowserRouter>
  );
};
