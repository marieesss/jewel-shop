export function DecoPanel() {
  return (
    <div className="relative h-full w-full overflow-hidden rounded-r-[32px]">
      {/* Dégradé de fond */}
      <div className="absolute inset-0 bg-[linear-gradient(160deg,#384959_0%,#4a6178_40%,#5a7a9a_70%,#6A89A7_100%)]" />

      {/* Marque centrée */}
      <div className="absolute inset-x-0 top-1/2 z-[2] -translate-y-1/2 text-center animate-[slide-up_0.8s_ease-out]">
        <h1 className="mb-2 font-display text-[56px] font-light leading-[1.1] tracking-[0.06em] text-white">
          Jewellery Shop
        </h1>
        <div className="mx-auto mb-3 h-px w-10 bg-flamant opacity-60" />
        <p className="mb-4 font-display text-[18px] italic text-poudre">
          Créez vos bijoux uniques
        </p>
        <p className="font-body text-xs uppercase tracking-[0.15em] text-mist">
          Or · Argent · Charmes
        </p>
      </div>
    </div>
  );
}
