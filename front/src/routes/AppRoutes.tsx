import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { LoginPage } from '../pages/LoginPage';
import { AppLayout } from '../components/layout/AppLayout';
import { MedicoDashboardPage } from '../pages/medico/MedicoDashboardPage';
import { AgendaMedica } from '../features/turnos/AgendaMedica';
import { HorariosMedicoPage } from '../pages/medico/HorariosMedicoPage';
import { HistoriaClinicaMedicoPage } from '../pages/medico/HistoriaClinicaMedicoPage';
import { PrescripcionesPage } from '../pages/medico/PrescripcionesPage';
import { RecepcionistaDashboardPage } from '../pages/recepcionista/RecepcionistaDashboardPage';
import { RecepcionistaLayout } from '../components/layout/RecepcionistaLayout';
import { RecepcionistaTurnosPage, RecepcionistaPacientesPage, RecepcionistaMedicosPage, RecepcionistaConsultoriosPage } from '../pages/recepcionista/RecepcionistaOperacionPages';
import { AdministradorDashboardPage } from '../pages/administrador/AdministradorDashboardPage';
import { NoAutorizadoPage } from '../pages/NoAutorizadoPage';
import { MisTurnos } from '../features/turnos/MisTurnos';
import { ProtectedRoute } from './ProtectedRoute';

export const AppRoutes: React.FC = () => {
  return (
    <BrowserRouter>
      <Routes>
        {/* Ruta de Login institucional */}
        <Route path="/login" element={<LoginPage />} />

        {/* Pantalla de respaldo para roles no reconocidos */}
        <Route path="/no-autorizado" element={<NoAutorizadoPage />} />

        {/* Módulo de Paciente (Turnos de Paciente - Desarrollado por compañero) */}
        <Route path="/mis-turnos" element={<MisTurnos idPacienteInicial={1} />} />

        {/* Portal del Profesional Médico protegido exclusivamente para médicos */}
        <Route element={<ProtectedRoute allowedRoles={['Medico']} />}>
          <Route path="/medico" element={<AppLayout />}>
            <Route index element={<MedicoDashboardPage />} />
            <Route path="turnos" element={<AgendaMedica />} />
            <Route path="horarios" element={<HorariosMedicoPage />} />
            <Route path="historia-clinica" element={<HistoriaClinicaMedicoPage />} />
            <Route path="prescripciones" element={<PrescripcionesPage />} />
            <Route path="*" element={<Navigate to="/medico" replace />} />
          </Route>
        </Route>

        {/* Portal de Recepción protegido para Recepcionista */}
        <Route element={<ProtectedRoute allowedRoles={['Recepcionista', 'Administrador']} />}>
          <Route path="/recepcionista" element={<RecepcionistaLayout />}>
            <Route index element={<RecepcionistaDashboardPage />} />
            <Route path="turnos" element={<RecepcionistaTurnosPage />} />
            <Route path="pacientes" element={<RecepcionistaPacientesPage />} />
            <Route path="medicos" element={<RecepcionistaMedicosPage />} />
            <Route path="consultorios" element={<RecepcionistaConsultoriosPage />} />
            <Route path="*" element={<Navigate to="/recepcionista" replace />} />
          </Route>
        </Route>

        {/* Portal de Administración protegido para Administrador */}
        <Route element={<ProtectedRoute allowedRoles={['Administrador']} />}>
          <Route path="/administrador" element={<AdministradorDashboardPage />} />
          <Route path="/administrador/*" element={<Navigate to="/administrador" replace />} />
        </Route>

        {/* Redirección por defecto al login */}
        <Route path="/" element={<Navigate to="/login" replace />} />
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    </BrowserRouter>
  );
};
