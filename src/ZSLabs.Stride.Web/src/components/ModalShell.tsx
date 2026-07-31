import type { FormEventHandler, ReactNode } from 'react'

interface ModalShellProps {
  labelledBy?: string
  header: ReactNode
  children: ReactNode
  onDismiss: () => void
  onSubmit?: FormEventHandler<HTMLFormElement>
}

export function ModalShell({ labelledBy, header, children, onDismiss, onSubmit }: ModalShellProps) {
  return (
    <div className="fixed inset-0 z-50 bg-stride-ink/35 md:grid md:place-items-center md:p-6" role="presentation" onMouseDown={onDismiss}>
      <form
        aria-labelledby={labelledBy}
        className="flex h-svh w-full flex-col overflow-hidden bg-white shadow-board md:max-h-[90vh] md:w-[90vw] md:max-w-[960px] md:rounded-[1.75rem] md:border md:border-stride-border"
        role="dialog"
        aria-modal="true"
        onMouseDown={(event) => event.stopPropagation()}
        onSubmit={onSubmit}
      >
        <header className="flex flex-col gap-4 border-b border-stride-border bg-stride-surface px-5 py-4 md:flex-row md:items-start md:justify-between md:px-6">
          {header}
        </header>
        <div className="min-h-0 flex-1 overflow-y-auto overflow-x-hidden px-5 py-5 md:px-6">
          {children}
        </div>
      </form>
    </div>
  )
}