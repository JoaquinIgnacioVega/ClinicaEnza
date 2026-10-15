export default function App() {
  return (
    <main className="flex min-h-screen items-center justify-center p-4">
      {/* Equivalente a Form1 (800x450) sin funcionalidades adicionales */}
      <section className="w-full max-w-[800px] min-h-[450px] bg-white rounded-lg shadow-md border border-slate-200 flex flex-col overflow-hidden">
        <header className="bg-slate-800 text-white px-4 py-2 text-sm font-medium flex items-center justify-between select-none">
          <span>ClinicaEnza - Form1</span>
          <div className="flex space-x-2">
            <span className="w-3 h-3 bg-slate-600 rounded-full inline-block"></span>
            <span className="w-3 h-3 bg-slate-600 rounded-full inline-block"></span>
            <span className="w-3 h-3 bg-slate-600 rounded-full inline-block"></span>
          </div>
        </header>
        <div className="flex-1 p-6 flex items-center justify-center text-slate-500">
          <p className="text-sm">Form1</p>
        </div>
      </section>
    </main>
  );
}
