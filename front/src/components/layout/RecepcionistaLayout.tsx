import React from 'react';
import { Outlet } from 'react-router-dom';
import { Topbar } from './Topbar';
import { RecepcionistaSidebar } from './RecepcionistaSidebar';

export const RecepcionistaLayout: React.FC = () => <div className="min-h-screen bg-slate-50 flex"><div className="hidden lg:block sticky top-0 h-screen"><RecepcionistaSidebar /></div><div className="flex-1 flex flex-col min-w-0"><Topbar /><main className="flex-1 p-6 sm:p-8 lg:p-10 max-w-7xl mx-auto w-full"><Outlet /></main></div></div>;
