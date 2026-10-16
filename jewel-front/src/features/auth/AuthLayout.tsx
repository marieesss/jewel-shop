import type { ReactNode } from 'react';
import { DecoPanel } from '../../components/DecoPanel';

interface AuthLayoutProps {
  children: ReactNode;
  /** Change de valeur pour rejouer l'animation d'entrée du contenu. */
  animationKey?: number;
}

export function AuthLayout({ children, animationKey }: AuthLayoutProps) {
  return (
    <div className="flex h-screen w-screen overflow-hidden bg-creme">
      {/* Panneau décoratif gauche */}
      <div className="flex h-full w-[42%] min-w-[380px] shrink-0 animate-[fade-in_0.6s_ease-out]">
        <DecoPanel />
      </div>

      {/* Panneau formulaire droite */}
      <div className="relative flex flex-1 items-start justify-center overflow-auto p-8">
        <div
          key={animationKey}
          className="my-auto w-full max-w-[420px] animate-[slide-up_0.5s_ease-out]"
        >
          {children}
        </div>
      </div>
    </div>
  );
}
