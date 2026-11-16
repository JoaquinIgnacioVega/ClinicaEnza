import React, { useCallback, useEffect, useState } from 'react';
import { Edit3, Plus, RefreshCw, Search, Shield, UserCheck, UserX, X } from 'lucide-react';
import { usuarioService } from '../../api/usuarioService';
import type { CrearUsuarioRequest, ModificarUsuarioRequest, Usuario } from '../../types/usuario';
import type { ApiError } from '../../api/client';

const roles = [
  { value: 1, label: 'Recepcionista' },
  { value: 2, label: 'Médico' },
];

const emptyForm: CrearUsuarioRequest = {
  nombre: '', apellido: '', dni: '', fechaNacimiento: '', mail: '', legajo: '', password: '', rol: 1, idMedico: null,
};

export const GestionUsuariosPage: React.FC = () => {
  const [usuarios, setUsuarios] = useState<Usuario[]>([]);
  const [soloActivos, setSoloActivos] = useState(true);
  const [busqueda, setBusqueda] = useState('');
  const [form, setForm] = useState<CrearUsuarioRequest>(emptyForm);
  const [editando, setEditando] = useState<Usuario | null>(null);
  const [mostrarFormulario, setMostrarFormulario] = useState(false);
  const [cargando, setCargando] = useState(true);
  const [guardando, setGuardando] = useState(false);
  const [mensaje, setMensaje] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const cargarUsuarios = useCallback(async () => {
    setCargando(true);
    setError(null);
    try {
      setUsuarios(await usuarioService.obtenerTodos(soloActivos));
    } catch (err) {
      setError((err as ApiError).error || 'No fue posible cargar los usuarios.');
    } finally {
      setCargando(false);
    }
  }, [soloActivos]);

  useEffect(() => { cargarUsuarios(); }, [cargarUsuarios]);

  const abrirAlta = () => {
    setEditando(null); setForm(emptyForm); setMostrarFormulario(true); setMensaje(null); setError(null);
  };

  const abrirEdicion = (usuario: Usuario) => {
    setEditando(usuario);
    setForm({ ...emptyForm, nombre: usuario.nombre, apellido: usuario.apellido, mail: usuario.mail, rol: usuario.rol, idMedico: usuario.idMedico });
    setMostrarFormulario(true); setMensaje(null); setError(null);
  };

  const cerrarFormulario = () => { if (!guardando) setMostrarFormulario(false); };
  const actualizarCampo = (campo: keyof CrearUsuarioRequest, valor: string | number) => setForm((actual) => ({ ...actual, [campo]: valor }));

  const guardar = async (event: React.FormEvent) => {
    event.preventDefault(); setGuardando(true); setError(null); setMensaje(null);
    try {
      if (editando) {
        const request: ModificarUsuarioRequest = { nombre: form.nombre, apellido: form.apellido, mail: form.mail, rol: form.rol, idMedico: form.idMedico };
        await usuarioService.modificar(editando.idUsuario, request);
        setMensaje('Usuario actualizado correctamente.');
      } else {
        await usuarioService.crear(form);
        setMensaje('Usuario creado correctamente.');
      }
      setMostrarFormulario(false); await cargarUsuarios();
    } catch (err) {
      setError((err as ApiError).error || 'No fue posible guardar el usuario.');
    } finally { setGuardando(false); }
  };

  const desactivar = async (usuario: Usuario) => {
    if (!window.confirm(`¿Desactivar a ${usuario.nombre} ${usuario.apellido}?`)) return;
    try { await usuarioService.darDeBaja(usuario.idUsuario); setMensaje('Usuario desactivado correctamente.'); await cargarUsuarios(); }
    catch (err) { setError((err as ApiError).error || 'No fue posible desactivar el usuario.'); }
  };

  const lista = usuarios.filter((usuario) => `${usuario.nombre} ${usuario.apellido} ${usuario.mail} ${usuario.legajo}`.toLowerCase().includes(busqueda.toLowerCase()));

  return (
    <div className="min-h-screen bg-slate-50 p-6 sm:p-8 lg:p-10">
      <div className="max-w-7xl mx-auto space-y-6">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
          <div><div className="flex items-center gap-2 text-xs font-bold text-[#0d5c68] uppercase tracking-wider"><Shield className="w-4 h-4" /> Superadministración</div><h1 className="text-2xl sm:text-3xl font-bold text-slate-900 mt-1">Gestión de usuarios</h1><p className="text-sm text-slate-500 mt-1">Alta, modificación, consulta y baja lógica de usuarios del sistema.</p></div>
          <button onClick={abrirAlta} className="inline-flex items-center justify-center gap-2 rounded-xl bg-[#0d5c68] px-4 py-2.5 text-sm font-semibold text-white hover:bg-[#0a4852]"><Plus className="w-4 h-4" />Nuevo usuario</button>
        </div>

        {mensaje && <div className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">{mensaje}</div>}
        {error && <div className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</div>}

        <div className="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden">
          <div className="p-4 border-b border-slate-100 flex flex-col md:flex-row gap-3 md:items-center md:justify-between">
            <div className="relative flex-1 max-w-md"><Search className="absolute left-3 top-3 w-4 h-4 text-slate-400" /><input value={busqueda} onChange={(e) => setBusqueda(e.target.value)} placeholder="Buscar por nombre, correo o legajo" className="w-full rounded-xl border border-slate-200 pl-9 pr-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-[#0d5c68]/20" /></div>
            <div className="flex items-center gap-3"><label className="flex items-center gap-2 text-sm text-slate-600"><input type="checkbox" checked={soloActivos} onChange={(e) => setSoloActivos(e.target.checked)} />Solo activos</label><button onClick={cargarUsuarios} className="rounded-lg border border-slate-200 p-2 text-slate-500 hover:bg-slate-50" title="Actualizar"><RefreshCw className="w-4 h-4" /></button></div>
          </div>
          {cargando ? <div className="p-10 text-center text-slate-500">Cargando usuarios...</div> : <div className="overflow-x-auto"><table className="w-full text-sm"><thead className="bg-slate-50 text-left text-xs uppercase tracking-wide text-slate-500"><tr><th className="px-5 py-3">Usuario</th><th className="px-5 py-3">Correo</th><th className="px-5 py-3">Legajo</th><th className="px-5 py-3">Rol</th><th className="px-5 py-3">Estado</th><th className="px-5 py-3 text-right">Acciones</th></tr></thead><tbody className="divide-y divide-slate-100">{lista.map((usuario) => <tr key={usuario.idUsuario} className="hover:bg-slate-50/70"><td className="px-5 py-4"><div className="font-semibold text-slate-800">{usuario.nombre} {usuario.apellido}</div><div className="text-xs text-slate-400">DNI {usuario.dni}</div></td><td className="px-5 py-4 text-slate-600">{usuario.mail}</td><td className="px-5 py-4 text-slate-600">{usuario.legajo}</td><td className="px-5 py-4"><span className="rounded-full bg-slate-100 px-2.5 py-1 text-xs font-semibold text-slate-600">{usuario.rolNombre}</span></td><td className="px-5 py-4">{usuario.activo ? <span className="inline-flex items-center gap-1 text-emerald-600"><UserCheck className="w-4 h-4" />Activo</span> : <span className="inline-flex items-center gap-1 text-slate-400"><UserX className="w-4 h-4" />Inactivo</span>}</td><td className="px-5 py-4 text-right"><div className="inline-flex gap-2"><button onClick={() => abrirEdicion(usuario)} className="rounded-lg border border-slate-200 p-2 text-slate-500 hover:bg-slate-50" title="Editar"><Edit3 className="w-4 h-4" /></button>{usuario.activo && <button onClick={() => desactivar(usuario)} className="rounded-lg border border-rose-200 p-2 text-rose-500 hover:bg-rose-50" title="Desactivar"><UserX className="w-4 h-4" /></button>}</div></td></tr>)}{lista.length === 0 && <tr><td colSpan={6} className="px-5 py-10 text-center text-slate-500">No se encontraron usuarios.</td></tr>}</tbody></table></div>}
        </div>

        {mostrarFormulario && <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 p-4"><div className="w-full max-w-2xl max-h-[90vh] overflow-y-auto rounded-2xl bg-white shadow-xl"><div className="flex items-center justify-between border-b border-slate-100 p-5"><h2 className="text-lg font-bold text-slate-900">{editando ? 'Editar usuario' : 'Nuevo usuario'}</h2><button onClick={cerrarFormulario} className="text-slate-400 hover:text-slate-700"><X className="w-5 h-5" /></button></div><form onSubmit={guardar} className="grid grid-cols-1 sm:grid-cols-2 gap-4 p-5"><label className="text-sm text-slate-600">Nombre<input required value={form.nombre} onChange={(e) => actualizarCampo('nombre', e.target.value)} className="mt-1 w-full rounded-lg border border-slate-200 px-3 py-2.5" /></label><label className="text-sm text-slate-600">Apellido<input required value={form.apellido} onChange={(e) => actualizarCampo('apellido', e.target.value)} className="mt-1 w-full rounded-lg border border-slate-200 px-3 py-2.5" /></label>{!editando && <><label className="text-sm text-slate-600">DNI<input required value={form.dni} onChange={(e) => actualizarCampo('dni', e.target.value)} className="mt-1 w-full rounded-lg border border-slate-200 px-3 py-2.5" /></label><label className="text-sm text-slate-600">Fecha de nacimiento<input required type="date" value={form.fechaNacimiento} onChange={(e) => actualizarCampo('fechaNacimiento', e.target.value)} className="mt-1 w-full rounded-lg border border-slate-200 px-3 py-2.5" /></label><label className="text-sm text-slate-600">Legajo<input required value={form.legajo} onChange={(e) => actualizarCampo('legajo', e.target.value)} className="mt-1 w-full rounded-lg border border-slate-200 px-3 py-2.5" /></label><label className="text-sm text-slate-600">Contraseña inicial<input required type="password" value={form.password} onChange={(e) => actualizarCampo('password', e.target.value)} className="mt-1 w-full rounded-lg border border-slate-200 px-3 py-2.5" /></label></>}<label className="text-sm text-slate-600">Correo<input required type="email" value={form.mail} onChange={(e) => actualizarCampo('mail', e.target.value)} className="mt-1 w-full rounded-lg border border-slate-200 px-3 py-2.5" /></label><label className="text-sm text-slate-600">Rol<select value={form.rol} onChange={(e) => actualizarCampo('rol', Number(e.target.value))} className="mt-1 w-full rounded-lg border border-slate-200 px-3 py-2.5">{roles.map((rol) => <option key={rol.value} value={rol.value}>{rol.label}</option>)}</select></label><div className="sm:col-span-2 flex justify-end gap-3 pt-2"><button type="button" onClick={cerrarFormulario} className="rounded-lg border border-slate-200 px-4 py-2.5 text-sm font-semibold text-slate-600">Cancelar</button><button disabled={guardando} className="rounded-lg bg-[#0d5c68] px-4 py-2.5 text-sm font-semibold text-white disabled:opacity-60">{guardando ? 'Guardando...' : editando ? 'Guardar cambios' : 'Crear usuario'}</button></div></form></div></div>}
      </div>
    </div>
  );
};
