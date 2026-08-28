import { useEffect, useMemo, useState, type Dispatch, type SetStateAction } from 'react'
import {
  BriefcaseBusiness,
  CircleDollarSign,
  FileText,
  LayoutDashboard,
  RefreshCw,
  ShieldCheck,
  Workflow,
  Zap,
} from 'lucide-react'
import WorkflowBuilder from './WorkflowBuilder'

type CustomerSummary = {
  id: string
  name: string
  company?: string | null
  email?: string | null
  status?: string
}

type ProposalWorkflowPanelProps = {
  opportunity: OpportunitySummary | null
  customers: CustomerSummary[]
  proposal: ProposalSummary | null
  lines: ProposalLineSummary[]
  approvals: ApprovalSummary[]
  workflow: WorkflowExecutionSummary | null
  order: OrderSummary | null
  subscription: SubscriptionSummary | null
  voucherCode: string
  vouchers: VoucherSummary[]
  message: string | null
  correctionReason: string
  setVoucherCode: Dispatch<SetStateAction<string>>
  setCorrectionReason: Dispatch<SetStateAction<string>>
  setLines: Dispatch<SetStateAction<ProposalLineSummary[]>>
  applyVoucher: () => void
  correctProposal: () => void
  resubmitProposal: () => void
  decideApproval: (approval: ApprovalSummary, decision: 'Approved' | 'Rejected') => void
  resumeCustomerApproval: () => void
}

function ProposalWorkflowPanel(props: ProposalWorkflowPanelProps) {
  const { opportunity, customers, proposal, lines, approvals, workflow, order, subscription, voucherCode, vouchers, message, correctionReason, setVoucherCode, setCorrectionReason, setLines, applyVoucher, correctProposal, resubmitProposal, decideApproval, resumeCustomerApproval } = props
  const proposalStatus = normalizeEnumValue(proposal?.status, proposalStatusMap)
  const workflowStatus = normalizeEnumValue(workflow?.status, workflowStatusMap)
  const pendingApproval = approvals.find((approval) => normalizeEnumValue(approval.status, approvalStatusMap) === 'Pending')
  const editable = proposalStatus === 'Draft' || proposalStatus === 'PendingApproval' || proposalStatus === 'Rejected' || proposalStatus === 'CustomerRejected'
  return <section className="workspace-stack">
    <button type="button" className="builder-back detail-back" onClick={() => window.location.assign('/')}><span>←</span> Back</button>
    {message ? <div className="panel panel-error" role="status"><p>{message}</p></div> : null}
    <div className="detail-grid">
      <article className="panel"><div className="panel-header"><div><p className="eyebrow">Opportunity</p><h3>{opportunity?.name ?? 'Loading...'}</h3></div><span className={`status ${getStatusClass(normalizeEnumValue(opportunity?.status, opportunityStatusMap))}`}>{formatValue(normalizeEnumValue(opportunity?.status, opportunityStatusMap))}</span></div><div className="detail-facts"><div><span>Customer</span><strong>{opportunity ? getCustomerName(opportunity.customerId, customers) : 'Loading...'}</strong></div><div><span>Proposal</span><strong>{proposal ? `${proposal.proposalNumber} V${proposal.revision ?? 1}` : 'Not created'}</strong></div><div><span>Total</span><strong>${(proposal?.totalAmount ?? 0).toLocaleString()}</strong></div></div></article>
      <article className="panel"><div className="panel-header"><h3>Workflow progress</h3><span className={`status ${getStatusClass(workflowStatus)}`}>{formatValue(workflowStatus)}</span></div><ul className="progress-list"><li className="done">✓ Opportunity</li><li className="done">✓ Proposal V{proposal?.revision ?? 1}</li><li className={proposalStatus === 'Rejected' ? 'rejected' : workflowStatus === 'Waiting' ? 'current' : 'done'}>{proposalStatus === 'Rejected' ? '✕' : workflowStatus === 'Waiting' ? '→' : '✓'} Manager Approval</li><li className={proposalStatus === 'CustomerApproved' || workflowStatus === 'Completed' ? 'done' : ''}>{proposalStatus === 'CustomerApproved' || workflowStatus === 'Completed' ? '✓' : '○'} Customer Approval</li><li className={order ? 'done' : ''}>{order ? '✓' : '○'} Order</li><li className={subscription ? 'done' : ''}>{subscription ? '✓' : '○'} Subscription</li></ul></article>
    </div>
    {proposal ? <article className="panel"><div className="panel-header"><div><p className="eyebrow">Pricing</p><h3>{proposalStatus} proposal</h3></div><span className={`status ${getStatusClass(proposalStatus)}`}>{formatValue(proposalStatus)}</span></div><div className="table-wrap"><table><thead><tr><th>Product</th><th>Qty</th><th>Unit price</th><th>Discount</th><th>Line total</th></tr></thead><tbody>{lines.length ? lines.map((line) => <tr key={line.id}><td>{line.productId.slice(0, 8)}</td><td>{editable ? <input type="number" min="1" value={line.quantity} onChange={(event) => setLines(current => current.map(item => item.id === line.id ? { ...item, quantity: Number(event.target.value) } : item))} /> : line.quantity}</td><td>${line.unitPrice.toLocaleString()}</td><td>{line.discountPercentage}%</td><td>${line.totalPrice.toLocaleString()}</td></tr>) : <tr><td colSpan={5}>No product lines.</td></tr>}</tbody></table></div><div className="detail-facts"><div><span>Subtotal</span><strong>${proposal.subTotal.toLocaleString()}</strong></div><div><span>Line discount</span><strong>${proposal.discountAmount.toLocaleString()}</strong></div><div><span>Voucher discount</span><strong>${(proposal.voucherDiscountAmount ?? 0).toLocaleString()}</strong></div><div><span>Final total</span><strong>${proposal.totalAmount.toLocaleString()}</strong></div></div>{editable ? <div className="form-actions"><select value={voucherCode} onChange={(event) => setVoucherCode(event.target.value)}><option value="">Select voucher</option>{vouchers.map(voucher => <option key={voucher.id} value={voucher.code}>{voucher.code}</option>)}</select><button type="button" className="primary-btn compact" onClick={() => void applyVoucher()}>Apply voucher</button></div> : null}</article> : null}
    {proposal && proposalStatus === 'Rejected' ? <article className="panel"><div className="panel-header"><h3>Correction</h3><span>Rejected proposal V{proposal.revision ?? 1}</span></div><p>Rejected by {approvals.find(approval => normalizeEnumValue(approval.status, approvalStatusMap) === 'Rejected')?.approverName ?? 'approver'}.</p><textarea value={correctionReason} onChange={(event) => setCorrectionReason(event.target.value)} placeholder="Correction reason" rows={3} /><div className="form-actions"><button type="button" className="primary-btn compact" onClick={() => void correctProposal()}>Correct Proposal</button></div></article> : null}
    {proposal ? <article className="panel"><div className="panel-header"><h3>Approval history</h3><span>{approvals.length} records</span></div><div className="record-list">{approvals.length ? approvals.map(approval => <div className="approval-row" key={approval.id}><div><strong>Cycle {approval.proposalRevision ?? 1} · V{approval.proposalRevision ?? 1}</strong><small>{approval.approverName} · {formatValue(normalizeEnumValue(approval.status, approvalStatusMap))} · {approval.respondedAt ? new Date(approval.respondedAt).toLocaleString() : new Date(approval.requestedAt).toLocaleString()}</small>{approval.comments ? <small>{approval.comments}</small> : null}</div>{normalizeEnumValue(approval.status, approvalStatusMap) === 'Pending' ? <div className="approval-actions"><button type="button" className="approve-btn" onClick={() => void decideApproval(approval, 'Approved')}>Approve</button><button type="button" className="reject-btn" onClick={() => void decideApproval(approval, 'Rejected')}>Reject</button></div> : null}</div>) : <p className="empty-state-inline">No approval history.</p>}</div>{proposalStatus === 'Draft' ? <button type="button" className="primary-btn compact" onClick={() => void resubmitProposal()}>Resubmit Proposal</button> : null}{proposalStatus === 'Approved' && workflow?.currentStepId ? <button type="button" className="primary-btn compact" onClick={() => void resumeCustomerApproval()}>Approve Customer</button> : null}</article> : null}
  </section>
}

type EmployeeSummary = {
  id: string
  firstName: string
  lastName: string
  email: string
  isActive: boolean
}

type PlayBookSummary = {
  id: string
  name: string
  version: number
  status: string
  triggerType: string
  createdBy: string
}

type OpportunitySummary = {
  id: string
  customerId: string
  assignedEmployeeId?: string | null
  name: string
  description?: string | null
  estimatedValue: number
  status: string
  expectedCloseDate?: string | null
}

type ProposalSummary = {
  id: string
  opportunityId: string
  customerId: string
  createdByEmployeeId: string
  proposalNumber: string
  status: string
  subTotal: number
  discountPercentage: number
  discountAmount: number
  voucherDiscountAmount?: number
  voucherCode?: string | null
  revision?: number
  totalAmount: number
  validUntil?: string | null
}

type OrderSummary = {
  id: string
  proposalId: string
  customerId: string
  assignedEmployeeId?: string | null
  orderNumber: string
  status: string
  totalAmount: number
  orderDate: string
}

type SubscriptionSummary = {
  id: string
  customerId: string
  productId: string
  startDate: string
  endDate: string
  amount: number
  status: string
}

type ApprovalSummary = {
  id: string
  proposalId: string
  workflowExecutionId?: string | null
  approverEmployeeId: string
  approverName: string
  approvalLevel: number
  proposalRevision?: number
  status: string | number
  comments?: string | null
  requestedAt: string
  respondedAt?: string | null
}

type ProposalLineSummary = {
  id: string
  proposalId: string
  productId: string
  quantity: number
  unitPrice: number
  discountPercentage: number
  discountAmount: number
  totalPrice: number
  discountType?: number
  discountValue?: number
}

type VoucherSummary = {
  id: string
  code: string
  discountType: number
  discountValue: number
  isActive: boolean
  validFrom?: string | null
  validUntil?: string | null
  minimumAmount?: number | null
  stackable: boolean
}

type WorkflowExecutionSummary = {
  id: string
  playBookId: string
  entityType: string
  entityId: string
  currentStepId?: string | null
  status: string | number
  errorMessage?: string | null
}

type DashboardData = {
  playbooks: PlayBookSummary[]
  customers: CustomerSummary[]
  opportunities: OpportunitySummary[]
  proposals: ProposalSummary[]
  orders: OrderSummary[]
  subscriptions: SubscriptionSummary[]
  approvals: ApprovalSummary[]
  employees: EmployeeSummary[]
}

const API_BASE_URL = 'http://localhost:5146/api'

const quickActions = [
  { label: 'Create proposal', icon: FileText },
  { label: 'Start workflow', icon: Zap },
  { label: 'Review approvals', icon: ShieldCheck },
  { label: 'Refresh data', icon: RefreshCw },
]

const formatValue = (value: string | number | null | undefined) => {
  const text = String(value ?? 'Unknown')
  return text.replace(/([a-z0-9])([A-Z])/g, '$1 $2').replace(/_/g, ' ').trim()
}

const normalizeEnumValue = (
  value: string | number | null | undefined,
  map?: Record<number, string>,
): string => {
  if (value == null) return 'Unknown'

  if (typeof value === 'number') {
    if (map && map[value] !== undefined) return map[value]
    return String(value)
  }

  if (map) {
    const numericValue = Number(value)
    if (!Number.isNaN(numericValue) && map[numericValue] !== undefined) {
      return map[numericValue]
    }
  }

  return value
}

const playbookStatusMap: Record<number, string> = {
  0: 'Draft',
  1: 'Active',
  2: 'Inactive',
  3: 'Archived',
}

const proposalStatusMap: Record<number, string> = {
  0: 'Draft',
  1: 'Submitted',
  2: 'PendingApproval',
  3: 'Approved',
  4: 'Rejected',
  5: 'CustomerPending',
  6: 'CustomerApproved',
  7: 'CustomerRejected',
  8: 'Expired',
}

const subscriptionStatusMap: Record<number, string> = {
  0: 'Active',
  1: 'Expiring',
  2: 'Expired',
  3: 'Renewed',
  4: 'Cancelled',
}

const approvalStatusMap: Record<number, string> = {
  0: 'Pending',
  1: 'Approved',
  2: 'Rejected',
}

const opportunityStatusMap: Record<number, string> = {
  0: 'New', 1: 'InProgress', 2: 'Proposal', 3: 'Approval', 4: 'CustomerApproval', 5: 'Won', 6: 'Lost', 7: 'Closed',
}

const workflowStatusMap: Record<number, string> = {
  0: 'Running', 1: 'Waiting', 2: 'Completed', 3: 'Failed', 4: 'Cancelled',
}

const getCustomerName = (customerId: string, customers: CustomerSummary[]) => {
  const customer = customers.find((item) => item.id === customerId)
  return customer?.name ?? 'Unknown customer'
}

const getStatusClass = (status: string) => {
  const normalized = status.toLowerCase()
  if (normalized.includes('waiting') || normalized.includes('pending') || normalized.includes('approval')) return 'waiting'
  if (normalized.includes('running')) return 'running'
  if (normalized.includes('completed') || normalized.includes('approved') || normalized.includes('won')) return 'completed'
  return 'waiting'
}

export default function App() {
  const opportunityPathId = window.location.pathname.match(/^\/opportunities\/([^/]+)$/)?.[1] ?? null
  const [activeView, setActiveView] = useState(opportunityPathId ? 'opportunity-detail' : 'dashboard')
  const [selectedOpportunityId, setSelectedOpportunityId] = useState<string | null>(opportunityPathId)
  const [showOpportunityForm, setShowOpportunityForm] = useState(false)
  const [opportunityForm, setOpportunityForm] = useState({ name: '', customerId: '', assignedEmployeeId: '', estimatedValue: '', status: 'New' })
  const [opportunityMessage, setOpportunityMessage] = useState<string | null>(null)
  const [workflowDetail, setWorkflowDetail] = useState<WorkflowExecutionSummary | null>(null)
  const [selectedOpportunity, setSelectedOpportunity] = useState<OpportunitySummary | null>(null)
  const [selectedProposal, setSelectedProposal] = useState<ProposalSummary | null>(null)
  const [selectedApproval, setSelectedApproval] = useState<ApprovalSummary | null>(null)
  const [proposalLines, setProposalLines] = useState<ProposalLineSummary[]>([])
  const [proposalApprovals, setProposalApprovals] = useState<ApprovalSummary[]>([])
  const [vouchers, setVouchers] = useState<VoucherSummary[]>([])
  const [voucherCode, setVoucherCode] = useState('')
  const [proposalMessage, setProposalMessage] = useState<string | null>(null)
  const [correctionReason, setCorrectionReason] = useState('')
  const [dataVersion, setDataVersion] = useState(0)
  const [data, setData] = useState<DashboardData>({
    playbooks: [],
    customers: [],
    opportunities: [],
    proposals: [],
    orders: [],
    subscriptions: [],
    approvals: [],
    employees: [],
  })
  const [selectedPlayBookId, setSelectedPlayBookId] = useState('')
  const [entityType, setEntityType] = useState('Proposal')
  const [entityId, setEntityId] = useState('')
  const [payloadText, setPayloadText] = useState('{"decision":"Approved"}')
  const [executionId, setExecutionId] = useState('')
  const [decision, setDecision] = useState('Approved')
  const [actionMessage, setActionMessage] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let isActive = true

    const loadDashboard = async () => {
      try {
        setLoading(true)
        setError(null)

        const [playbooksRes, customersRes, opportunitiesRes, proposalsRes, ordersRes, subscriptionsRes, employeesRes] = await Promise.all([
          fetch(`${API_BASE_URL}/workflows/playbooks`),
          fetch(`${API_BASE_URL}/crm/customers`),
          fetch(`${API_BASE_URL}/crm/opportunities`),
          fetch(`${API_BASE_URL}/crm/proposals`),
          fetch(`${API_BASE_URL}/crm/orders`),
          fetch(`${API_BASE_URL}/crm/subscriptions`),
          fetch(`${API_BASE_URL}/crm/employees`),
        ])

        if ([playbooksRes, customersRes, opportunitiesRes, proposalsRes, ordersRes, subscriptionsRes, employeesRes].some((res) => !res.ok)) {
          throw new Error('Unable to load the dashboard data from the API.')
        }

        const [playbooks, customers, opportunities, proposals, orders, subscriptions, employees] = await Promise.all([
          playbooksRes.json() as Promise<PlayBookSummary[]>,
          customersRes.json() as Promise<CustomerSummary[]>,
          opportunitiesRes.json() as Promise<OpportunitySummary[]>,
          proposalsRes.json() as Promise<ProposalSummary[]>,
          ordersRes.json() as Promise<OrderSummary[]>,
          subscriptionsRes.json() as Promise<SubscriptionSummary[]>,
          employeesRes.json() as Promise<EmployeeSummary[]>,
        ])

        const approvals = (await Promise.all(proposals.map(async (proposal) => {
          const response = await fetch(`${API_BASE_URL}/approvals/proposals/${proposal.id}`)
          return response.ok ? response.json() as Promise<ApprovalSummary[]> : []
        }))).flat()

        if (!isActive) return

        setData({
          playbooks,
          customers,
          opportunities,
          proposals,
          orders,
          subscriptions,
          approvals,
          employees,
        })

        if (!selectedPlayBookId && playbooks.length > 0) {
          setSelectedPlayBookId(playbooks[0].id)
        }
      } catch (loadError) {
        if (!isActive) return
        setError(loadError instanceof Error ? loadError.message : 'Unable to load dashboard data.')
      } finally {
        if (isActive) setLoading(false)
      }
    }

    void loadDashboard()
    return () => {
      isActive = false
    }
  }, [selectedPlayBookId, dataVersion])

  useEffect(() => {
    if (!selectedOpportunityId) return
    let active = true
    const loadOpportunityDetail = async () => {
      try {
        const opportunityResponse = await fetch(`${API_BASE_URL}/crm/opportunities/${selectedOpportunityId}`)
        if (!opportunityResponse.ok) throw new Error('Unable to load this Opportunity.')
        const opportunity = await opportunityResponse.json() as OpportunitySummary
        const proposalsResponse = await fetch(`${API_BASE_URL}/crm/proposals`)
        const proposals = await proposalsResponse.json() as ProposalSummary[]
        const proposal = proposals.find((item) => item.opportunityId === selectedOpportunityId) ?? null
        let approval: ApprovalSummary | null = null
        let execution: WorkflowExecutionSummary | null = null
        if (proposal) {
          const approvalsResponse = await fetch(`${API_BASE_URL}/approvals/proposals/${proposal.id}`)
          const approvals = approvalsResponse.ok ? await approvalsResponse.json() as ApprovalSummary[] : []
          const linesResponse = await fetch(`${API_BASE_URL}/crm/proposals/${proposal.id}/products`)
          const lines = linesResponse.ok ? await linesResponse.json() as ProposalLineSummary[] : []
          approval = [...approvals].reverse().find((item) => item.workflowExecutionId) ?? null
          if (approval?.workflowExecutionId) {
            const executionResponse = await fetch(`${API_BASE_URL}/workflows/executions/${approval.workflowExecutionId}`)
            execution = executionResponse.ok ? await executionResponse.json() as WorkflowExecutionSummary : null
          }
          setProposalApprovals(approvals)
          setProposalLines(lines)
          setVoucherCode(proposal.voucherCode ?? '')
        }
        if (!active) return
        setSelectedOpportunity(opportunity)
        setSelectedProposal(proposal)
        setSelectedApproval(approval)
        setWorkflowDetail(execution)
        const vouchersResponse = await fetch(`${API_BASE_URL}/crm/vouchers`)
        if (vouchersResponse.ok) setVouchers(await vouchersResponse.json() as VoucherSummary[])
      } catch (detailError) {
        if (active) setOpportunityMessage(detailError instanceof Error ? detailError.message : 'Unable to load Opportunity details.')
      }
    }
    void loadOpportunityDetail()
    const poll = window.setInterval(() => void loadOpportunityDetail(), 2500)
    return () => { active = false; window.clearInterval(poll) }
  }, [selectedOpportunityId, dataVersion])

  const createOpportunity = async () => {
    if (!opportunityForm.name.trim() || !opportunityForm.customerId || !opportunityForm.estimatedValue) {
      setOpportunityMessage('Opportunity name, customer, and estimated value are required.')
      return
    }
    try {
      setOpportunityMessage(null)
      const response = await fetch(`${API_BASE_URL}/crm/opportunities`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          name: opportunityForm.name.trim(),
          customerId: opportunityForm.customerId,
          assignedEmployeeId: opportunityForm.assignedEmployeeId || null,
          estimatedValue: Number(opportunityForm.estimatedValue),
          status: Math.max(0, ['New', 'InProgress', 'Proposal', 'Approval', 'CustomerApproval', 'Won', 'Lost', 'Closed'].indexOf(opportunityForm.status)),
        }),
      })
      const responseText = await response.text()
      let result: OpportunitySummary
      try { result = JSON.parse(responseText) as OpportunitySummary } catch { throw new Error(responseText || 'Unable to create Opportunity.') }
      if (!response.ok) throw new Error((result as OpportunitySummary & { title?: string; detail?: string }).title ?? (result as OpportunitySummary & { title?: string; detail?: string }).detail ?? 'Unable to create Opportunity.')
      window.history.pushState({}, '', `/opportunities/${result.id}`)
      setSelectedOpportunityId(result.id)
      setActiveView('opportunity-detail')
      setShowOpportunityForm(false)
      setOpportunityForm({ name: '', customerId: '', assignedEmployeeId: '', estimatedValue: '', status: 'New' })
      setOpportunityMessage('Opportunity created. The workflow engine is processing it.')
      setDataVersion((version) => version + 1)
    } catch (createError) {
      setOpportunityMessage(createError instanceof Error ? createError.message : 'Unable to create Opportunity.')
    }
  }

  const startWorkflow = async () => {
    if (!selectedPlayBookId || !entityId.trim()) {
      setActionMessage('Select a PlayBook and enter an entity ID to start the workflow.')
      return
    }

    try {
      const parsedPayload = payloadText.trim() ? JSON.parse(payloadText) : {}
      const response = await fetch(`${API_BASE_URL}/workflows/executions`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          playBookId: selectedPlayBookId,
          entityType,
          entityId,
          payload: parsedPayload,
        }),
      })

      const result = await response.json()
      if (!response.ok) {
        throw new Error(result?.errorMessage ?? result?.title ?? 'Unable to start workflow.')
      }

      setExecutionId(result.id)
      setActionMessage(`Workflow started: ${normalizeEnumValue(result.status)} (${result.id})`)
    } catch (err) {
      setActionMessage(err instanceof Error ? err.message : 'Workflow start failed.')
    }
  }

  const resumeWorkflow = async () => {
    if (!executionId.trim()) {
      setActionMessage('Enter an execution ID before resuming the workflow.')
      return
    }

    try {
      const response = await fetch(`${API_BASE_URL}/workflows/executions/${executionId}/resume`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ payload: { decision } }),
      })

      const result = await response.json()
      if (!response.ok) {
        throw new Error(result?.errorMessage ?? result?.title ?? 'Unable to resume workflow.')
      }

      setActionMessage(`Workflow resumed: ${normalizeEnumValue(result.status)} (${result.id})`)
    } catch (err) {
      setActionMessage(err instanceof Error ? err.message : 'Workflow resume failed.')
    }
  }

  const decideApproval = async (approval: ApprovalSummary, nextStatus: 'Approved' | 'Rejected') => {
    try {
      const response = await fetch(`${API_BASE_URL}/approvals/${approval.id}/decision`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          approverEmployeeId: approval.approverEmployeeId,
          decision: nextStatus === 'Approved' ? 1 : 2,
          comments: `${nextStatus} from PlayBook console.`,
        }),
      })

      if (!response.ok) {
        throw new Error('Unable to record the approval decision.')
      }

      if (nextStatus === 'Approved' && approval.workflowExecutionId) {
        const resumeResponse = await fetch(`${API_BASE_URL}/workflows/executions/${approval.workflowExecutionId}/resume`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ payload: { decision: nextStatus } }),
        })
        if (!resumeResponse.ok) throw new Error('Decision saved, but the workflow could not be resumed.')
      }

      setActionMessage(`Approval ${approval.id.slice(0, 8).toUpperCase()} marked ${nextStatus} and workflow resumed.`)
      window.location.reload()
    } catch (err) {
      setActionMessage(err instanceof Error ? err.message : 'Approval decision failed.')
    }
  }

  const resumeSpecificWorkflow = async (workflowId: string, nextDecision: 'Approved' | 'Rejected') => {
    const response = await fetch(`${API_BASE_URL}/workflows/executions/${workflowId}/resume`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ payload: { decision: nextDecision } }) })
    if (!response.ok) throw new Error(await response.text() || 'Workflow could not be resumed.')
    refreshWorkflowData()
  }

  const resumeCustomerApproval = async () => {
    if (!workflowDetail?.id) return
    await resumeSpecificWorkflow(workflowDetail.id, 'Approved')
  }

  const refreshWorkflowData = () => setDataVersion((version) => version + 1)

  const applyVoucher = async () => {
    if (!selectedProposal || !voucherCode.trim()) return
    try {
      setProposalMessage(null)
      const validation = await fetch(`${API_BASE_URL}/crm/vouchers/${encodeURIComponent(voucherCode.trim())}/validate?amount=${selectedProposal.subTotal - selectedProposal.discountAmount}`)
      if (!validation.ok) throw new Error(await validation.text() || 'Voucher could not be applied.')
      const response = await fetch(`${API_BASE_URL}/crm/proposals/${selectedProposal.id}`, {
        method: 'PUT', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ opportunityId: selectedProposal.opportunityId, customerId: selectedProposal.customerId, createdByEmployeeId: selectedProposal.createdByEmployeeId, proposalNumber: selectedProposal.proposalNumber, status: Object.entries(proposalStatusMap).find(([, value]) => value === selectedProposal.status)?.[0] ?? selectedProposal.status, validUntil: selectedProposal.validUntil, voucherCode: voucherCode.trim(), products: proposalLines.map((line) => ({ productId: line.productId, quantity: line.quantity, discountType: line.discountType ?? 0, discountValue: line.discountValue || line.discountPercentage || selectedProposal.discountPercentage })) }),
      })
      if (!response.ok) throw new Error(await response.text() || 'Proposal pricing could not be recalculated.')
      setProposalMessage('Voucher applied. Totals were recalculated by the server.')
      refreshWorkflowData()
    } catch (error) {
      setProposalMessage(error instanceof Error ? error.message : 'Voucher could not be applied.')
    }
  }

  const correctProposal = async () => {
    if (!selectedProposal) return
    try {
      const response = await fetch(`${API_BASE_URL}/crm/proposals/${selectedProposal.id}/correct`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ reason: correctionReason.trim() || 'Correction requested.' }) })
      if (!response.ok) throw new Error(await response.text() || 'Proposal could not be corrected.')
      setProposalMessage('Proposal revision created. Update the proposal and resubmit it for approval.')
      refreshWorkflowData()
    } catch (error) {
      setProposalMessage(error instanceof Error ? error.message : 'Proposal correction failed.')
    }
  }

  const resubmitProposal = async () => {
    if (!selectedProposal) return
    try {
      const response = await fetch(`${API_BASE_URL}/approvals/proposals/${selectedProposal.id}/resubmit`, { method: 'POST' })
      if (!response.ok) throw new Error(await response.text() || 'Proposal could not be resubmitted.')
      setProposalMessage('Proposal resubmitted. A new approval cycle is now active.')
      refreshWorkflowData()
    } catch (error) {
      setProposalMessage(error instanceof Error ? error.message : 'Proposal resubmission failed.')
    }
  }

  const metrics = useMemo(() => {
    const activeOpportunities = data.opportunities.filter((item) => !['Closed', 'Lost'].includes(normalizeEnumValue(item.status))).length
    const pendingApprovals = data.proposals.filter((item) => ['PendingApproval', 'CustomerPending'].includes(normalizeEnumValue(item.status, proposalStatusMap))).length
    const openOrders = data.orders.filter((item) => !['Completed', 'Cancelled'].includes(normalizeEnumValue(item.status))).length
    const runningWorkflows = data.playbooks.filter((item) => normalizeEnumValue(item.status, playbookStatusMap) === 'Active').length
    const totalPipeline = data.opportunities.reduce((sum, item) => sum + item.estimatedValue, 0)

    return {
      activeOpportunities,
      pendingApprovals,
      openOrders,
      runningWorkflows,
      totalPipeline,
    }
  }, [data])

  const workflowRows = useMemo(
    () =>
      data.playbooks.slice(0, 4).map((playBook) => ({
        id: playBook.id.slice(0, 8).toUpperCase(),
        playbookId: playBook.id,
        customer: getCustomerName(data.customers[0]?.id ?? '', data.customers),
        playbook: playBook.name,
        step: formatValue(playBook.triggerType),
        status: normalizeEnumValue(playBook.status, playbookStatusMap),
        owner: playBook.createdBy,
      })),
    [data],
  )

  const renewalRows = useMemo(
    () =>
      data.subscriptions
        .filter((row) => ['Active', 'Expiring'].includes(normalizeEnumValue(row.status, subscriptionStatusMap)))
        .slice(0, 3)
        .map((row) => ({
          customer: getCustomerName(row.customerId, data.customers),
          date: new Date(row.endDate).toLocaleDateString(undefined, { month: 'short', day: 'numeric' }),
          amount: `$${row.amount.toLocaleString()}`,
        })),
    [data],
  )

  const recentActivities = useMemo(() => {
    const activities: string[] = []

    data.playbooks.slice(0, 2).forEach((playBook) => {
      activities.push(`${playBook.name} is ${formatValue(normalizeEnumValue(playBook.status, playbookStatusMap))}.`)
    })

    data.proposals.slice(0, 2).forEach((proposal) => {
      activities.push(`Proposal ${proposal.proposalNumber} is ${formatValue(normalizeEnumValue(proposal.status, proposalStatusMap))}.`)
    })

    data.subscriptions.slice(0, 2).forEach((subscription) => {
      activities.push(`${getCustomerName(subscription.customerId, data.customers)} subscription is ${formatValue(normalizeEnumValue(subscription.status, subscriptionStatusMap))}.`)
    })

    return activities.slice(0, 4)
  }, [data])

  const statCards = [
    { label: 'Active opportunities', value: String(metrics.activeOpportunities), hint: '$' + metrics.totalPipeline.toLocaleString() + ' pipeline', icon: BriefcaseBusiness, tone: 'cyan' },
    { label: 'Pending approvals', value: String(metrics.pendingApprovals), hint: 'Needs review', icon: ShieldCheck, tone: 'amber' },
    { label: 'Open orders', value: String(metrics.openOrders), hint: 'Live order queue', icon: CircleDollarSign, tone: 'green' },
    { label: 'Running workflows', value: String(metrics.runningWorkflows), hint: 'Operational flows', icon: Workflow, tone: 'violet' },
  ]

  const pendingApprovals = data.approvals.filter((approval) => normalizeEnumValue(approval.status, approvalStatusMap).toLowerCase().includes('pending'))
  const proposalCustomer = (proposal: ProposalSummary) => getCustomerName(proposal.customerId, data.customers)

  const renderWorkspace = () => {
    if (activeView === 'proposals') {
      return (
        <section className="workspace-grid">
          <article className="panel workspace-panel">
            <div className="panel-header"><div><p className="eyebrow">Revenue operations</p><h3>Proposal queue</h3></div><span className="pill neutral">{data.proposals.length} records</span></div>
            <div className="record-list">
              {data.proposals.map((proposal) => (
                <button key={proposal.id} type="button" className="record-row" onClick={() => { setEntityId(proposal.id); setActiveView('approvals') }}>
                  <span><strong>{proposal.proposalNumber}</strong><small>{proposalCustomer(proposal)}</small></span>
                  <span><strong>${proposal.totalAmount.toLocaleString()}</strong><small>{proposal.voucherCode ? `${proposal.voucherCode} · ` : ''}V${proposal.revision ?? 1} · ${proposal.voucherDiscountAmount ?? 0} voucher discount</small><small className={`status ${getStatusClass(normalizeEnumValue(proposal.status, proposalStatusMap))}`}>{formatValue(normalizeEnumValue(proposal.status, proposalStatusMap))}</small></span>
                </button>
              ))}
            </div>
          </article>
          <article className="panel workspace-panel">
            <div className="panel-header"><h3>Proposal signals</h3></div>
            <div className="mini-metrics"><div><span>Total value</span><strong>${data.proposals.reduce((sum, proposal) => sum + proposal.totalAmount, 0).toLocaleString()}</strong></div><div><span>Awaiting approval</span><strong>{data.proposals.filter((proposal) => normalizeEnumValue(proposal.status, proposalStatusMap) === 'PendingApproval').length}</strong></div></div>
          </article>
        </section>
      )
    }

    if (activeView === 'approvals') {
      return (
        <section className="workspace-grid">
          <article className="panel workspace-panel">
            <div className="panel-header"><div><p className="eyebrow">Decision center</p><h3>Approval queue</h3></div><span className="pill neutral">{pendingApprovals.length} pending</span></div>
            <div className="record-list">
              {pendingApprovals.length === 0 ? <p className="empty-state-inline">No pending approvals.</p> : pendingApprovals.map((approval) => {
                const proposal = data.proposals.find((item) => item.id === approval.proposalId)
                return <div key={approval.id} className="approval-row"><div><strong>{proposal?.proposalNumber ?? 'Proposal'}</strong><small>{proposal ? proposalCustomer(proposal) : 'Unknown customer'} · Level {approval.approvalLevel} · {approval.approverName}</small></div><div className="approval-actions"><button type="button" className="approve-btn" onClick={() => void decideApproval(approval, 'Approved')}>Approve</button><button type="button" className="reject-btn" onClick={() => void decideApproval(approval, 'Rejected')}>Reject</button></div></div>
              })}
            </div>
          </article>
        </section>
      )
    }

    if (activeView === 'workflows') {
      return (
        <section className="workspace-grid">
          <article className="panel workspace-panel">
            <div className="panel-header"><div><p className="eyebrow">Automation catalog</p><h3>Active PlayBooks</h3></div><button type="button" className="action-btn compact" onClick={() => window.location.assign('/playbooks/new')}>New PlayBook</button></div>
            <div className="record-list">
              {data.playbooks.map((playBook) => <button key={playBook.id} type="button" className="workflow-card" onClick={() => window.location.assign(`/playbooks/${playBook.id}/edit`)}><div><strong>{playBook.name}</strong><small>v{playBook.version} · Trigger: {formatValue(playBook.triggerType)}</small></div><span className={`status ${getStatusClass(normalizeEnumValue(playBook.status, playbookStatusMap))}`}>{formatValue(normalizeEnumValue(playBook.status, playbookStatusMap))}</span></button>)}
            </div>
          </article>
        </section>
      )
    }

    if (activeView === 'crm') {
      return (
        <section className="workspace-stack">
          <article className="panel workspace-panel">
            <div className="panel-header"><div><p className="eyebrow">Customer relationships</p><h3>Opportunities</h3></div><button type="button" className="primary-btn compact" onClick={() => { setShowOpportunityForm(true); setOpportunityMessage(null) }}>+ New Opportunity</button></div>
            {showOpportunityForm ? <div className="opportunity-form">
              <label className="builder-field"><span>Opportunity name</span><input value={opportunityForm.name} onChange={(event) => setOpportunityForm({ ...opportunityForm, name: event.target.value })} placeholder="e.g. Connectivity expansion" /></label>
              <label className="builder-field"><span>Customer</span><select value={opportunityForm.customerId} onChange={(event) => setOpportunityForm({ ...opportunityForm, customerId: event.target.value })}><option value="">Select customer</option>{data.customers.map((customer) => <option key={customer.id} value={customer.id}>{customer.name}</option>)}</select></label>
              <label className="builder-field"><span>Assigned employee</span><select value={opportunityForm.assignedEmployeeId} onChange={(event) => setOpportunityForm({ ...opportunityForm, assignedEmployeeId: event.target.value })}><option value="">Unassigned</option>{data.employees.filter((employee) => employee.isActive).map((employee) => <option key={employee.id} value={employee.id}>{employee.firstName} {employee.lastName}</option>)}</select></label>
              <label className="builder-field"><span>Estimated value</span><input type="number" min="0" value={opportunityForm.estimatedValue} onChange={(event) => setOpportunityForm({ ...opportunityForm, estimatedValue: event.target.value })} /></label>
              <label className="builder-field"><span>Status</span><select value={opportunityForm.status} onChange={(event) => setOpportunityForm({ ...opportunityForm, status: event.target.value })}>{['New', 'InProgress', 'Proposal', 'Approval', 'CustomerApproval', 'Won', 'Lost', 'Closed'].map((status) => <option key={status}>{formatValue(status)}</option>)}</select></label>
              <div className="form-actions"><button type="button" className="action-btn" onClick={() => setShowOpportunityForm(false)}>Cancel</button><button type="button" className="primary-btn compact" onClick={() => void createOpportunity()}>Create Opportunity</button></div>
            </div> : null}
            {opportunityMessage && showOpportunityForm ? <p className="form-message">{opportunityMessage}</p> : null}
            <div className="table-wrap"><table><thead><tr><th>Opportunity</th><th>Customer</th><th>Assigned employee</th><th>Value</th><th>Status</th><th>Workflow</th></tr></thead><tbody>{data.opportunities.map((opportunity) => <tr key={opportunity.id} className="clickable-row" onClick={() => { window.history.pushState({}, '', `/opportunities/${opportunity.id}`); setSelectedOpportunityId(opportunity.id); setActiveView('opportunity-detail') }}><td><strong>{opportunity.name}</strong></td><td>{getCustomerName(opportunity.customerId, data.customers)}</td><td>{data.employees.find((employee) => employee.id === opportunity.assignedEmployeeId)?.firstName ?? 'Unassigned'}</td><td>${opportunity.estimatedValue.toLocaleString()}</td><td><span className={`status ${getStatusClass(normalizeEnumValue(opportunity.status, opportunityStatusMap))}`}>{formatValue(normalizeEnumValue(opportunity.status, opportunityStatusMap))}</span></td><td><span className="pill neutral">View details</span></td></tr>)}</tbody></table></div>
          </article>
        </section>
      )
    }

    if (activeView === 'opportunity-detail') {
      const relatedOrder = selectedProposal ? data.orders.find((order) => order.proposalId === selectedProposal.id) ?? null : null
      const relatedSubscription = selectedOpportunity ? data.subscriptions.find((subscription) => subscription.customerId === selectedOpportunity.customerId) ?? null : null
      return <ProposalWorkflowPanel opportunity={selectedOpportunity} customers={data.customers} proposal={selectedProposal} lines={proposalLines} approvals={proposalApprovals} workflow={workflowDetail} order={relatedOrder} subscription={relatedSubscription} voucherCode={voucherCode} vouchers={vouchers} message={proposalMessage} correctionReason={correctionReason} setVoucherCode={setVoucherCode} setCorrectionReason={setCorrectionReason} setLines={setProposalLines} applyVoucher={applyVoucher} correctProposal={correctProposal} resubmitProposal={resubmitProposal} decideApproval={decideApproval} resumeCustomerApproval={resumeCustomerApproval} />
      /*
      const customerApprovalCompleted = normalizeEnumValue(selectedProposal?.status, proposalStatusMap) === 'CustomerApproved' || normalizeEnumValue(workflowDetail?.status, workflowStatusMap) === 'Completed'
      return <section className="workspace-stack"><button type="button" className="builder-back detail-back" onClick={() => { setActiveView('crm'); window.history.pushState({}, '', '/') }}><span>←</span> Back to CRM</button>{opportunityMessage ? <div className="panel panel-error"><p>{opportunityMessage}</p></div> : null}<div className="detail-grid"><article className="panel"><div className="panel-header"><div><p className="eyebrow">Opportunity</p><h3>{selectedOpportunity?.name ?? 'Loading...'}</h3></div><span className={`status ${getStatusClass(normalizeEnumValue(selectedOpportunity?.status, opportunityStatusMap))}`}>{formatValue(normalizeEnumValue(selectedOpportunity?.status, opportunityStatusMap))}</span></div><div className="detail-facts"><div><span>Customer</span><strong>{selectedOpportunity ? getCustomerName(selectedOpportunity.customerId, data.customers) : 'Loading...'}</strong></div><div><span>Assigned employee</span><strong>{selectedOpportunity ? data.employees.find((employee) => employee.id === selectedOpportunity.assignedEmployeeId)?.firstName ?? 'Unassigned' : 'Loading...'}</strong></div><div><span>Estimated value</span><strong>${selectedOpportunity?.estimatedValue.toLocaleString() ?? '0'}</strong></div></div></article><article className="panel"><div className="panel-header"><h3>Workflow progress</h3><span className={`status ${getStatusClass(normalizeEnumValue(workflowDetail?.status, workflowStatusMap))}`}>{formatValue(normalizeEnumValue(workflowDetail?.status, workflowStatusMap))}</span></div><ul className="progress-list"><li className="done">✓ Opportunity created</li><li className={selectedProposal ? 'done' : 'current'}>{selectedProposal ? '✓' : '→'} Proposal created</li><li className={selectedApproval?.status === 0 ? 'current' : selectedApproval ? 'done' : ''}>{selectedApproval?.status === 0 ? '→' : selectedApproval ? '✓' : '○'} Manager approval</li><li className={customerApprovalCompleted ? 'done' : selectedApproval && selectedApproval.status !== 0 ? 'current' : ''}>{customerApprovalCompleted ? '✓' : selectedApproval && selectedApproval.status !== 0 ? '→' : '○'} Customer approval</li><li className={relatedOrder ? 'done' : ''}>{relatedOrder ? '✓' : '○'} Order</li><li className={relatedSubscription ? 'done' : ''}>{relatedSubscription ? '✓' : '○'} Subscription</li></ul>{workflowDetail?.currentStepId ? <small className="muted-line">Current step ID: {workflowDetail.currentStepId}</small> : null}</article></div><article className="panel"><div className="panel-header"><h3>Related proposal</h3>{selectedProposal ? <span className="pill neutral">{formatValue(normalizeEnumValue(selectedProposal.status, proposalStatusMap))}</span> : null}</div>{selectedProposal ? <div className="detail-record"><div><strong>{selectedProposal.proposalNumber}</strong><small>{selectedProposal.discountPercentage}% discount · ${selectedProposal.totalAmount.toLocaleString()} total</small></div>{selectedApproval?.status === 0 && selectedApproval.workflowExecutionId ? <div className="approval-actions"><button type="button" className="approve-btn" onClick={() => void decideApproval(selectedApproval, 'Approved')}>Approve</button><button type="button" className="reject-btn" onClick={() => void decideApproval(selectedApproval, 'Rejected')}>Reject</button></div> : null}</div> : <p className="empty-state-inline">Waiting for the workflow to create a Proposal...</p>}</article></section>
      */
    }

    return null
  }

  const builderMatch = window.location.pathname.match(/^\/playbooks\/(new|([^/]+)\/edit)$/)
  if (builderMatch) {
    return <WorkflowBuilder playBookId={builderMatch[1] === 'new' ? undefined : builderMatch[2]} onBack={() => window.location.assign('/')} />
  }

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand-block">
          <div className="brand-mark">P</div>
          <div>
            <p className="eyebrow">Operations</p>
            <h2>PlayBook</h2>
          </div>
        </div>

        <nav className="nav">
          <button className={`nav-item ${activeView === 'dashboard' ? 'active' : ''}`} type="button" onClick={() => setActiveView('dashboard')}>
            <LayoutDashboard size={16} />
            Dashboard
          </button>
          <button className={`nav-item ${activeView === 'workflows' ? 'active' : ''}`} type="button" onClick={() => setActiveView('workflows')}>
            <Workflow size={16} />
            Workflows
          </button>
          <button className={`nav-item ${activeView === 'proposals' ? 'active' : ''}`} type="button" onClick={() => setActiveView('proposals')}>
            <FileText size={16} />
            Proposals
          </button>
          <button className={`nav-item ${activeView === 'crm' ? 'active' : ''}`} type="button" onClick={() => setActiveView('crm')}>
            <BriefcaseBusiness size={16} />
            CRM
          </button>
          <button className={`nav-item ${activeView === 'approvals' ? 'active' : ''}`} type="button" onClick={() => setActiveView('approvals')}>
            <ShieldCheck size={16} />
            Approvals
          </button>
        </nav>

        <div className="sidebar-card">
          <p className="label">System health</p>
          <strong>{loading ? '...' : '99.2%'}</strong>
          <span>{error ? 'API unavailable' : 'Workflow engine online'}</span>
        </div>
      </aside>

      <main className="main-panel">
        <header className="topbar">
          <div>
            <p className="eyebrow">Executive overview</p>
            <h1>{activeView === 'dashboard' ? 'CRM + workflow dashboard' : formatValue(activeView).replace(/^./, (character) => character.toUpperCase())}</h1>
          </div>
          <button type="button" className="primary-btn" onClick={() => window.location.reload()}>
            <RefreshCw size={16} />
            Refresh data
          </button>
        </header>

        {error ? (
          <div className="panel panel-error">
            <strong>Unable to load the dashboard</strong>
            <p>{error}</p>
          </div>
        ) : null}

        {activeView === 'dashboard' ? <section className="stats-grid">
          {statCards.map(({ label, value, hint, icon: Icon, tone }) => (
            <article key={label} className={`stat-card ${tone}`}>
              <div className="icon-wrap">
                <Icon size={18} />
              </div>
              <div>
                <span>{label}</span>
                <strong>{loading ? '...' : value}</strong>
                <small>{hint}</small>
              </div>
            </article>
          ))}
        </section> : null}

        {activeView !== 'dashboard' ? renderWorkspace() : null}

        {activeView === 'dashboard' ? <>
        <section className="quick-actions">
          {quickActions.map(({ label, icon: Icon }) => (
            <button key={label} type="button" className="action-btn">
              <Icon size={16} />
              {label}
            </button>
          ))}
        </section>

        <section className="panel action-panel">
          <div className="panel-header">
            <h3>Workflow actions</h3>
            <span className="pill neutral">Live API</span>
          </div>

          <div className="action-grid">
            <label>
              <span>PlayBook</span>
              <select value={selectedPlayBookId} onChange={(event) => setSelectedPlayBookId(event.target.value)}>
                {data.playbooks.map((playBook) => (
                  <option key={playBook.id} value={playBook.id}>{playBook.name}</option>
                ))}
              </select>
            </label>

            <label>
              <span>Entity type</span>
              <input value={entityType} onChange={(event) => setEntityType(event.target.value)} />
            </label>

            <label>
              <span>Entity ID</span>
              <input value={entityId} onChange={(event) => setEntityId(event.target.value)} placeholder="GUID for Proposal / Opportunity" />
            </label>

            <label className="full-width">
              <span>Payload JSON</span>
              <textarea value={payloadText} onChange={(event) => setPayloadText(event.target.value)} rows={4} />
            </label>

            <div className="action-actions">
              <button type="button" className="primary-btn compact" onClick={startWorkflow}>Start workflow</button>
            </div>

            <label>
              <span>Execution ID</span>
              <input value={executionId} onChange={(event) => setExecutionId(event.target.value)} placeholder="Execution GUID" />
            </label>

            <label>
              <span>Resume decision</span>
              <select value={decision} onChange={(event) => setDecision(event.target.value)}>
                <option value="Approved">Approved</option>
                <option value="Rejected">Rejected</option>
              </select>
            </label>

            <div className="action-actions">
              <button type="button" className="action-btn compact" onClick={resumeWorkflow}>Resume workflow</button>
            </div>
          </div>

          {actionMessage ? <div className="action-status" aria-live="polite">{actionMessage}</div> : null}
        </section>

        <section className="content-grid">
          <article className="panel large-panel">
            <div className="panel-header">
              <h3>Recently active workflows</h3>
              <span className="pill neutral">Live</span>
            </div>
            <div className="table-wrap">
              <table>
                <thead>
                  <tr>
                    <th>Workflow</th>
                    <th>Customer</th>
                    <th>Playbook</th>
                    <th>Current step</th>
                    <th>Status</th>
                    <th>Owner</th>
                  </tr>
                </thead>
                <tbody>
                  {workflowRows.length === 0 ? (
                    <tr>
                      <td colSpan={6} className="empty-state">{loading ? 'Loading workflow data...' : 'No workflows available.'}</td>
                    </tr>
                  ) : (
                    workflowRows.map((row) => (
                      <tr key={row.playbookId}>
                        <td>{row.id}</td>
                        <td>{row.customer}</td>
                        <td>{row.playbook}</td>
                        <td>{row.step}</td>
                        <td><span className={`status ${getStatusClass(row.status)}`}>{formatValue(row.status)}</span></td>
                        <td>{row.owner}</td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          </article>

          <article className="panel">
            <div className="panel-header">
              <h3>Upcoming renewals</h3>
              <span className="pill neutral">Next 7 days</span>
            </div>
            <ul className="list-stack">
              {renewalRows.length === 0 ? (
                <li className="empty-state-inline">{loading ? 'Loading renewals...' : 'No renewals to display.'}</li>
              ) : (
                renewalRows.map((renewal, index) => (
                  <li key={`${renewal.customer}-${renewal.date}-${renewal.amount}-${index}`}>
                    <div>
                      <strong>{renewal.customer}</strong>
                      <small>{renewal.date}</small>
                    </div>
                    <span>{renewal.amount}</span>
                  </li>
                ))
              )}
            </ul>
          </article>
        </section>

        <section className="content-grid lower-grid">
          <article className="panel">
            <div className="panel-header">
              <h3>Recent activity</h3>
            </div>
            <ul className="activity-list">
              {recentActivities.length === 0 ? (
                <li className="empty-state-inline">{loading ? 'Loading activities...' : 'No recent activity.'}</li>
              ) : (
                recentActivities.map((activity) => (
                  <li key={activity}>
                    <span className="dot" />
                    {activity}
                  </li>
                ))
              )}
            </ul>
          </article>

          <article className="panel">
            <div className="panel-header">
              <h3>Pipeline snapshot</h3>
            </div>
            <div className="mini-metrics">
              <div>
                <span>Won this quarter</span>
                <strong>{loading ? '...' : '$' + data.opportunities.reduce((sum, opportunity) => sum + opportunity.estimatedValue, 0).toLocaleString()}</strong>
              </div>
              <div>
                <span>Proposal acceptance</span>
                <strong>{loading ? '...' : String(Math.min(99, Math.max(20, Math.round((data.proposals.filter((item) => item.status === 'Approved').length / Math.max(1, data.proposals.length)) * 100)))) + '%'}</strong>
              </div>
              <div>
                <span>Avg. approval time</span>
                <strong>{loading ? '...' : '2.4 days'}</strong>
              </div>
            </div>
          </article>
        </section>
        </> : null}
      </main>
    </div>
  )
}
