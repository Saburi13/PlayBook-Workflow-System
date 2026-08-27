import { useCallback, useEffect, useState } from 'react'
import ReactFlow, {
  addEdge,
  applyEdgeChanges,
  applyNodeChanges,
  Background,
  Controls,
  Handle,
  MiniMap,
  Position,
  type Connection,
  type Edge,
  type Node,
  type NodeProps,
  type OnNodesChange,
  type OnEdgesChange,
} from 'reactflow'
import 'reactflow/dist/style.css'
import { ArrowLeft, Check, CircleHelp, Clock3, GitBranch, MousePointer2, Save, Sparkles, Trash2, UserCheck, Zap } from 'lucide-react'

const API_BASE_URL = 'http://localhost:5146/api'

type BuilderNodeData = {
  label: string
  description: string
  stepType: string
  config: Record<string, string | number>
  isStartStep?: boolean
  isEndStep?: boolean
}

type PlayBookDetail = {
  id: string
  name: string
  description?: string | null
  version: number
  status: string | number
  triggerType: string | number
  createdBy: string
  steps: Array<{ id: string; name: string; description?: string | null; stepType: string | number; configurationJson?: string | null; positionX: number; positionY: number; isStartStep: boolean; isEndStep: boolean }>
  transitions: Array<{ id: string; fromStepId: string; toStepId: string; label?: string | null; priority: number; condition?: { field: string; operator: string | number; value?: string | null; dataType: string } | null }>
}

type PaletteItem = { type: string; label: string; icon: typeof Zap; description: string }

const palette: PaletteItem[] = [
  { type: 'Trigger', label: 'Trigger', icon: Zap, description: 'Start an automated flow' },
  { type: 'Action', label: 'Action', icon: Check, description: 'Perform a system action' },
  { type: 'Condition', label: 'Condition', icon: GitBranch, description: 'Branch on a rule' },
  { type: 'Approval', label: 'Approval', icon: UserCheck, description: 'Request a decision' },
  { type: 'CustomerAction', label: 'Customer Action', icon: MousePointer2, description: 'Wait for customer input' },
  { type: 'Notification', label: 'Notification', icon: Sparkles, description: 'Notify a team member' },
  { type: 'EmployeeAssignment', label: 'Employee Assignment', icon: UserCheck, description: 'Assign an owner' },
  { type: 'Wait', label: 'Wait', icon: Clock3, description: 'Pause the workflow' },
  { type: 'End', label: 'End', icon: Check, description: 'Complete the workflow' },
]

const enumName = (value: string | number, names: string[]) => typeof value === 'number' ? names[value] ?? String(value) : value
const stepTypeNames = ['Trigger', 'Action', 'Condition', 'Approval', 'Notification', 'CustomerAction', 'EmployeeAssignment', 'Wait', 'End']
const triggerTypes = ['Event', 'Date', 'Manual', 'Condition']
const actionTypes = ['Create Proposal', 'Create Order', 'Auto Approval', 'Update Status', 'Create Activity', 'Assign Employee', 'Send Notification']
const conditionOperators = ['Equals', 'NotEquals', 'GreaterThan', 'LessThan', 'GreaterThanOrEqual', 'LessThanOrEqual', 'Contains', 'StartsWith', 'EndsWith', 'IsNull', 'IsNotNull']

const enumValue = (value: string, values: string[]) => Math.max(0, values.indexOf(value))

const defaultsFor = (type: string): BuilderNodeData => {
  const common = { label: palette.find((item) => item.type === type)?.label ?? type, description: '', stepType: type, config: {} }
  if (type === 'Trigger') return { ...common, label: 'Opportunity Created', config: { triggerType: 'Event', event: 'Opportunity Created' }, isStartStep: true }
  if (type === 'Action') return { ...common, label: 'Create Proposal', config: { actionType: 'Create Proposal' } }
  if (type === 'Condition') return { ...common, label: 'Discount > 5%', config: { field: 'Proposal.DiscountPercentage', operator: 'GreaterThan', value: '5', dataType: 'decimal' } }
  if (type === 'Approval') return { ...common, label: 'Manager Approval', config: { approverType: 'Manager', approver: '', approvalLevel: 1 } }
  if (type === 'CustomerAction') return { ...common, label: 'Customer Approval', config: { action: 'Customer Approval' } }
  if (type === 'Wait') return { ...common, label: 'Wait', config: { duration: 1, unit: 'Days' } }
  if (type === 'End') return { ...common, label: 'Workflow Completed', isEndStep: true }
  return common
}

function WorkflowNode({ data, selected }: NodeProps<BuilderNodeData>) {
  const item = palette.find((paletteItem) => paletteItem.type === data.stepType) ?? palette[0]
  const Icon = item.icon
  return <div className={`flow-node node-${data.stepType.toLowerCase()} ${selected ? 'selected' : ''}`}>
    {data.stepType !== 'Trigger' ? <Handle type="target" position={Position.Left} /> : null}
    <div className="flow-node-top"><Icon size={15} /><span>{item.label}</span></div>
    <strong>{data.label}</strong>
    <small>{data.description || item.description}</small>
    {data.stepType !== 'End' ? <Handle type="source" position={Position.Right} /> : null}
  </div>
}

const nodeTypes = { workflow: WorkflowNode }

const parseConfig = (json?: string | null) => {
  if (!json) return {}
  try { return JSON.parse(json) as Record<string, string | number> } catch { return {} }
}

const toBuilderGraph = (detail: PlayBookDetail) => {
  const nodes: Node<BuilderNodeData>[] = detail.steps.map((step) => ({
    id: step.id,
    type: 'workflow',
    position: { x: step.positionX, y: step.positionY },
    data: (() => {
      const stepType = enumName(step.stepType, stepTypeNames)
      const condition = detail.transitions.find((transition) => transition.fromStepId === step.id)?.condition
      return { label: step.name, description: step.description ?? '', stepType, config: condition && stepType === 'Condition' ? { ...parseConfig(step.configurationJson), field: condition.field, operator: enumName(condition.operator, conditionOperators), value: condition.value ?? '', dataType: condition.dataType } : parseConfig(step.configurationJson), isStartStep: step.isStartStep, isEndStep: step.isEndStep }
    })(),
  }))
  const edges: Edge[] = detail.transitions.map((transition) => ({ id: transition.id, source: transition.fromStepId, target: transition.toStepId, label: transition.label ?? undefined, animated: transition.label === 'TRUE' || transition.label === 'FALSE' }))
  return { nodes, edges }
}

export default function WorkflowBuilder({ playBookId, onBack }: { playBookId?: string; onBack: () => void }) {
  const [currentPlayBookId, setCurrentPlayBookId] = useState(playBookId)
  const [name, setName] = useState('Untitled PlayBook')
  const [status, setStatus] = useState('Draft')
  const [createdBy, setCreatedBy] = useState('system')
  const [nodes, setNodes] = useState<Node<BuilderNodeData>[]>([])
  const [edges, setEdges] = useState<Edge[]>([])
  const [selectedNodeId, setSelectedNodeId] = useState<string | null>(null)
  const [preview, setPreview] = useState(false)
  const [message, setMessage] = useState('')
  const [loading, setLoading] = useState(Boolean(playBookId))

  useEffect(() => {
    if (!playBookId) {
      setNodes([{ id: crypto.randomUUID(), type: 'workflow', position: { x: 120, y: 160 }, data: defaultsFor('Trigger') }])
      return
    }
    void fetch(`${API_BASE_URL}/workflows/playbooks/${playBookId}`).then(async (response) => {
      if (!response.ok) throw new Error('Unable to load this PlayBook.')
      const detail = await response.json() as PlayBookDetail
      const graph = toBuilderGraph(detail)
      setName(detail.name); setStatus(enumName(detail.status, ['Draft', 'Active', 'Inactive', 'Archived'])); setCreatedBy(detail.createdBy); setNodes(graph.nodes); setEdges(graph.edges)
    }).catch((error: unknown) => setMessage(error instanceof Error ? error.message : 'Unable to load this PlayBook.')).finally(() => setLoading(false))
  }, [playBookId])

  const selectedNode = nodes.find((node) => node.id === selectedNodeId)
  const onNodesChange: OnNodesChange = useCallback((changes) => setNodes((current) => applyNodeChanges(changes, current)), [])
  const onEdgesChange: OnEdgesChange = useCallback((changes) => setEdges((current) => applyEdgeChanges(changes, current)), [])
  const onConnect = useCallback((connection: Connection) => setEdges((current) => addEdge({ ...connection, type: 'smoothstep' }, current)), [])

  const addNode = (type: string) => {
    const data = defaultsFor(type)
    if (data.isStartStep && nodes.some((node) => node.data.isStartStep)) { setMessage('A PlayBook can only have one Trigger.'); return }
    const id = crypto.randomUUID()
    setNodes((current) => [...current, { id, type: 'workflow', position: { x: 180 + current.length * 24, y: 120 + (current.length % 4) * 110 }, data }])
    setSelectedNodeId(id); setMessage('')
  }

  const updateSelected = (patch: Partial<BuilderNodeData>, configPatch?: Record<string, string | number>) => {
    if (!selectedNodeId) return
    setNodes((current) => current.map((node) => node.id === selectedNodeId ? { ...node, data: { ...node.data, ...patch, config: { ...node.data.config, ...configPatch } } } : node))
  }

  const removeSelected = () => {
    if (!selectedNodeId) return
    setNodes((current) => current.filter((node) => node.id !== selectedNodeId)); setEdges((current) => current.filter((edge) => edge.source !== selectedNodeId && edge.target !== selectedNodeId)); setSelectedNodeId(null)
  }

  const validate = () => {
    const errors: string[] = []
    if (!name.trim()) errors.push('PlayBook name is required.')
    if (!nodes.some((node) => node.data.stepType === 'Trigger')) errors.push('Add a Trigger node.')
    if (!nodes.some((node) => node.data.stepType === 'End')) errors.push('Add an End node.')
    if (nodes.some((node) => node.data.stepType !== 'Trigger' && !edges.some((edge) => edge.source === node.id))) errors.push('Connect every node to an incoming workflow path.')
    if (nodes.some((node) => node.data.stepType === 'Approval' && !node.data.config.approverType)) errors.push('Configure an approver for every Approval node.')
    return errors
  }

  const save = async (activate = false) => {
    const errors = validate()
    if (errors.length) { setMessage(errors.join(' ')); return }
    const graphNodes = nodes.map((node) => node.data)
    const request = {
      name: name.trim(), description: 'Designed in PlayBook Builder', status: 0, triggerType: enumValue(String(nodes.find((node) => node.data.stepType === 'Trigger')?.data.config.triggerType ?? 'Manual'), triggerTypes), createdBy: createdBy.trim() || 'system',
      steps: nodes.map((node) => ({ name: node.data.label, description: node.data.description || null, stepType: enumValue(node.data.stepType, stepTypeNames), configurationJson: JSON.stringify(node.data.config), positionX: node.position.x, positionY: node.position.y, isStartStep: Boolean(node.data.isStartStep), isEndStep: Boolean(node.data.isEndStep) })),
      transitions: (() => { const branchCounts: Record<string, number> = {}; return edges.map((edge, index) => { const source = nodes.findIndex((node) => node.id === edge.source); const sourceData = graphNodes[source]; const branch = branchCounts[edge.source] ?? 0; branchCounts[edge.source] = branch + 1; const label = edge.label?.toString() || (sourceData?.stepType === 'Condition' ? branch === 0 ? 'TRUE' : 'FALSE' : null); return { fromStepIndex: source, toStepIndex: nodes.findIndex((node) => node.id === edge.target), label, priority: index, condition: sourceData?.stepType === 'Condition' ? { field: String(sourceData.config.field ?? ''), operator: enumValue(String(sourceData.config.operator ?? 'Equals'), conditionOperators), value: String(sourceData.config.value ?? ''), dataType: String(sourceData.config.dataType ?? 'string') } : null } }) })(),
    }
    const response = await fetch(`${API_BASE_URL}/workflows/playbooks${currentPlayBookId ? `/${currentPlayBookId}` : ''}`, { method: currentPlayBookId ? 'PUT' : 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(request) })
    if (!response.ok) { setMessage(await response.text() || 'Unable to save PlayBook.'); return }
    const saved = await response.json() as { id: string; status: string | number }
    if (!currentPlayBookId) { setCurrentPlayBookId(saved.id); window.history.replaceState({}, '', `/playbooks/${saved.id}/edit`) }
    if (activate) {
      const activation = await fetch(`${API_BASE_URL}/workflows/playbooks/${saved.id}/activate`, { method: 'POST' })
      if (!activation.ok) { setMessage(await activation.text() || 'PlayBook was saved but could not be activated.'); return }
      setStatus('Active'); setMessage('PlayBook saved and activated.')
    } else { setStatus('Draft'); setMessage('Draft saved.') }
  }

  if (loading) return <div className="builder-loading">Loading PlayBook...</div>
  return <div className={`builder-shell ${preview ? 'preview-mode' : ''}`}>
    <header className="builder-toolbar">
      <button type="button" className="builder-back" onClick={onBack}><ArrowLeft size={16} /> Back to PlayBooks</button>
      <input className="builder-name" value={name} onChange={(event) => setName(event.target.value)} aria-label="PlayBook name" />
      <span className={`builder-status ${status.toLowerCase()}`}>{status}</span>
      <div className="builder-toolbar-actions"><button type="button" onClick={() => setPreview((current) => !current)}><CircleHelp size={15} /> {preview ? 'Edit' : 'Preview'}</button>{!preview ? <><button type="button" onClick={() => void save(false)}><Save size={15} /> Save Draft</button><button type="button" className="activate-action" onClick={() => void save(true)}>Activate</button></> : null}</div>
    </header>
    {message ? <div className="builder-message" role="status">{message}</div> : null}
    <main className="builder-layout">
      {!preview ? <aside className="node-palette"><div className="builder-section-title"><span>Node palette</span><small>Drag or click to add</small></div>{palette.map((item) => { const Icon = item.icon; return <button key={item.type} type="button" className="palette-item" draggable onDragStart={(event) => event.dataTransfer.setData('application/playbook-node', item.type)} onClick={() => addNode(item.type)}><Icon size={16} /><span><strong>{item.label}</strong><small>{item.description}</small></span></button> })}</aside> : null}
      <section className="builder-canvas" onDragOver={(event) => event.preventDefault()} onDrop={(event) => { const type = event.dataTransfer.getData('application/playbook-node'); if (type) addNode(type) }}>
        {nodes.length === 0 ? <div className="canvas-empty"><Sparkles size={24} /><strong>Build your workflow</strong><span>Add a node from the palette to begin.</span></div> : <ReactFlow nodes={nodes} edges={edges} nodeTypes={nodeTypes} onNodesChange={preview ? undefined : onNodesChange} onEdgesChange={preview ? undefined : onEdgesChange} onConnect={preview ? undefined : onConnect} onNodeClick={(_, node) => setSelectedNodeId(node.id)} fitView nodesDraggable={!preview} nodesConnectable={!preview} elementsSelectable><Background color="#334155" gap={24} /><Controls /><MiniMap nodeColor="#67e8f9" /></ReactFlow>}
      </section>
      {!preview ? <aside className="config-panel"><div className="builder-section-title"><span>Configuration</span><small>{selectedNode ? 'Selected node' : 'Select a node'}</small></div>{selectedNode ? <><label className="builder-field"><span>Node type</span><input value={selectedNode.data.stepType} disabled /></label><label className="builder-field"><span>Label</span><input value={selectedNode.data.label} onChange={(event) => updateSelected({ label: event.target.value })} /></label><label className="builder-field"><span>Description</span><textarea value={selectedNode.data.description} onChange={(event) => updateSelected({ description: event.target.value })} rows={3} /></label>{selectedNode.data.stepType === 'Trigger' ? <>{fieldFor('Trigger Type', 'triggerType', triggerTypes, selectedNode, updateSelected)}{fieldFor('Event', 'event', [], selectedNode, updateSelected)}</> : null}{selectedNode.data.stepType === 'Action' ? <>{fieldFor('Action Type', 'actionType', actionTypes, selectedNode, updateSelected)}</> : null}{selectedNode.data.stepType === 'Condition' ? <>{fieldFor('Field', 'field', ['Proposal.DiscountPercentage', 'Proposal.TotalAmount', 'Opportunity.EstimatedValue'], selectedNode, updateSelected)}{fieldFor('Operator', 'operator', conditionOperators, selectedNode, updateSelected)}{fieldFor('Value', 'value', [], selectedNode, updateSelected)}</> : null}{selectedNode.data.stepType === 'Approval' ? <>{fieldFor('Approver Type', 'approverType', ['Employee', 'Manager', 'Employee Grade'], selectedNode, updateSelected)}{fieldFor('Approver', 'approver', [], selectedNode, updateSelected)}{fieldFor('Approval Level', 'approvalLevel', [], selectedNode, updateSelected, 'number')}</> : null}{selectedNode.data.stepType === 'CustomerAction' ? fieldFor('Action', 'action', ['Customer Approval'], selectedNode, updateSelected) : null}{selectedNode.data.stepType === 'Wait' ? <>{fieldFor('Duration', 'duration', [], selectedNode, updateSelected, 'number')}{fieldFor('Unit', 'unit', ['Minutes', 'Hours', 'Days'], selectedNode, updateSelected)}</> : null}<button type="button" className="delete-node" onClick={removeSelected}><Trash2 size={15} /> Delete node</button></> : <div className="config-empty">Select a node on the canvas to configure it.</div>}</aside> : null}
    </main>
  </div>
}

function fieldFor(label: string, key: string, options: string[], node: Node<BuilderNodeData>, update: (patch: Partial<BuilderNodeData>, configPatch?: Record<string, string | number>) => void, type = 'text') {
  if (options.length > 0) return <label className="builder-field"><span>{label}</span><select value={String(node.data.config[key] ?? options[0])} onChange={(event) => update({}, { [key]: event.target.value })}>{options.map((option) => <option key={option}>{option}</option>)}</select></label>
  return <label className="builder-field"><span>{label}</span><input type={type} value={String(node.data.config[key] ?? '')} onChange={(event) => update({}, { [key]: type === 'number' ? Number(event.target.value) : event.target.value })} /></label>
}
