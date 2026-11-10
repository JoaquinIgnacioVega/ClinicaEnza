import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import {
  Stethoscope,
  Calendar,
  Clock,
  FileText,
  Pill,
  ArrowRight,
  Building2,
} from 'lucide-react';

export const MedicoDashboardPage: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useAuth();

  const nombreDoctor = user?.nombreCompleto || 'Dr. Juan Pérez';

  return (
    <div className="space-y-8 max-w-6xl mx-auto animate-fadeIn">
      {/* Encabezado */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <div className="flex items-center gap-2 text-xs font-bold text-[#0d5c68] uppercase tracking-wider">
            <Stethoscope className="w-4 h-4" />
            <span>Portal del Profesional Médico</span>
          </div>
          <h1 className="text-2xl sm:text-3xl font-bold text-slate-900 tracking-tight mt-1">
            Bienvenido/a, {nombreDoctor}
          </h1>
          <p className="text-xs sm:text-sm text-slate-500 mt-1">
            Panel de control para la atención de pacientes, historias clínicas, franjas horarias y prescripciones.
          </p>
        </div>

        <button
          type="button"
          onClick={() => navigate('/medico/turnos')}
          className="inline-flex items-center gap-2 px-5 py-2.5 rounded-xl bg-[#0d5c68] hover:bg-[#0a4852] text-white text-xs sm:text-sm font-semibold shadow-sm transition-all cursor-pointer self-start sm:self-auto active:scale-98"
        >
          <Calendar className="w-4 h-4" />
          <span>Ver Agenda de Hoy</span>
        </button>
      </div>

      {/* Tarjeta de Ficha Médica Activa */}
      <div className="bg-white rounded-2xl border border-slate-200/80 p-6 shadow-xs flex flex-col md:flex-row md:items-center justify-between gap-5">
        <div className="flex items-start sm:items-center gap-4">
          <div className="w-12 h-12 rounded-2xl bg-teal-50 text-[#0d5c68] flex items-center justify-center shrink-0 border border-teal-100">
            <Stethoscope className="w-6 h-6" />
          </div>
          <div className="space-y-1">
            <span className="text-[10px] font-bold tracking-wider text-teal-700 bg-teal-50 px-2 py-0.5 rounded-full border border-teal-100 uppercase">
              SESIÓN CLÍNICA ACTIVA
            </span>
            <h3 className="text-lg font-bold text-slate-900">
              {nombreDoctor}
            </h3>
            <p className="text-xs text-slate-500">
              Especialidad: <strong>Clínica Médica</strong> · Matrícula: <strong>MP-99999</strong> · Consultorio 1
            </p>
          </div>
        </div>

        <div className="flex items-center gap-3">
          <div className="px-4 py-2 rounded-xl bg-slate-50 border border-slate-200 text-xs text-slate-600 flex items-center gap-2">
            <Building2 className="w-4 h-4 text-[#0d5c68]" />
            <span>Sede Central (Consultorio 1)</span>
          </div>
        </div>
      </div>

      {/* Grid: 4 Módulos de Trabajo */}
      <div className="space-y-4">
        <h3 className="text-xs font-bold text-slate-500 uppercase tracking-wider">
          Módulos de Atención Médica
        </h3>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-5">
          {/* Módulo 1: Agenda */}
          <div
            onClick={() => navigate('/medico/turnos')}
            className="p-6 rounded-2xl bg-white border border-slate-200/80 hover:border-[#0d5c68]/40 hover:shadow-sm transition-all cursor-pointer group flex flex-col justify-between space-y-4"
          >
            <div className="space-y-3">
              <div className="w-11 h-11 rounded-xl bg-sky-50 text-sky-600 flex items-center justify-center group-hover:scale-105 transition-transform">
                <Calendar className="w-6 h-6" />
              </div>
              <h3 className="text-base font-bold text-slate-800 group-hover:text-[#0d5c68] transition-colors">
                Agenda de Turnos
              </h3>
              <p className="text-xs text-slate-500 leading-relaxed">
                Visualiza los turnos citados por fecha, consulta la historia del paciente y finaliza consultas marcándolas como atendidas.
              </p>
            </div>

            <div className="pt-3 border-t border-slate-100 flex items-center justify-between text-xs font-semibold text-[#0d5c68]">
              <span>Ingresar a la agenda</span>
              <ArrowRight className="w-4 h-4 group-hover:translate-x-1 transition-transform" />
            </div>
          </div>

          {/* Módulo 2: Historia Clínica (RF25) */}
          <div
            onClick={() => navigate('/medico/historia-clinica')}
            className="p-6 rounded-2xl bg-white border border-slate-200/80 hover:border-[#0d5c68]/40 hover:shadow-sm transition-all cursor-pointer group flex flex-col justify-between space-y-4"
          >
            <div className="space-y-3">
              <div className="w-11 h-11 rounded-xl bg-cyan-50 text-[#0d5c68] flex items-center justify-center group-hover:scale-105 transition-transform">
                <FileText className="w-6 h-6" />
              </div>
              <h3 className="text-base font-bold text-slate-800 group-hover:text-[#0d5c68] transition-colors">
                Historia Clínica
              </h3>
              <p className="text-xs text-slate-500 leading-relaxed">
                Requerimiento Funcional 25: busca pacientes por DNI o nombre y consulta sus antecedentes, biometría y prescripciones.
              </p>
            </div>

            <div className="pt-3 border-t border-slate-100 flex items-center justify-between text-xs font-semibold text-[#0d5c68]">
              <span>Consultar expediente</span>
              <ArrowRight className="w-4 h-4 group-hover:translate-x-1 transition-transform" />
            </div>
          </div>

          {/* Módulo 3: Horarios */}
          <div
            onClick={() => navigate('/medico/horarios')}
            className="p-6 rounded-2xl bg-white border border-slate-200/80 hover:border-[#0d5c68]/40 hover:shadow-sm transition-all cursor-pointer group flex flex-col justify-between space-y-4"
          >
            <div className="space-y-3">
              <div className="w-11 h-11 rounded-xl bg-teal-50 text-[#0d5c68] flex items-center justify-center group-hover:scale-105 transition-transform">
                <Clock className="w-6 h-6" />
              </div>
              <h3 className="text-base font-bold text-slate-800 group-hover:text-[#0d5c68] transition-colors">
                Horarios y Consultorio
              </h3>
              <p className="text-xs text-slate-500 leading-relaxed">
                Configura tus franjas de disponibilidad semanal, asocia consultorios y prueba la generación de slots de 20 minutos.
              </p>
            </div>

            <div className="pt-3 border-t border-slate-100 flex items-center justify-between text-xs font-semibold text-[#0d5c68]">
              <span>Configurar horarios</span>
              <ArrowRight className="w-4 h-4 group-hover:translate-x-1 transition-transform" />
            </div>
          </div>

          {/* Módulo 4: Prescripciones */}
          <div
            onClick={() => navigate('/medico/prescripciones')}
            className="p-6 rounded-2xl bg-white border border-slate-200/80 hover:border-[#0d5c68]/40 hover:shadow-sm transition-all cursor-pointer group flex flex-col justify-between space-y-4"
          >
            <div className="space-y-3">
              <div className="w-11 h-11 rounded-xl bg-emerald-50 text-emerald-600 flex items-center justify-center group-hover:scale-105 transition-transform">
                <Pill className="w-6 h-6" />
              </div>
              <h3 className="text-base font-bold text-slate-800 group-hover:text-[#0d5c68] transition-colors">
                Prescripciones Médicas
              </h3>
              <p className="text-xs text-slate-500 leading-relaxed">
                Requerimiento Funcional 22: registra recetas en la historia clínica del paciente y consulta indicaciones previas.
              </p>
            </div>

            <div className="pt-3 border-t border-slate-100 flex items-center justify-between text-xs font-semibold text-[#0d5c68]">
              <span>Gestionar recetas</span>
              <ArrowRight className="w-4 h-4 group-hover:translate-x-1 transition-transform" />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
