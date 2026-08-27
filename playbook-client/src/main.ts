import './style.css'

type PlayBookSummary = {
  id: string;
  name: string;
  version: number;
  status: string;
  triggerType: string;
  createdBy: string;
};

type SubscriptionSummary = {
  id: string;
  customerId: string;
  productId: string;
  startDate: string;
  endDate: string;
  amount: number;
  status: string;
};

type WorkflowExecution = {
  id: string;
  playBookId: string;
  entityType: string;
  entityId: string;
  currentStepId?: string | null;
  status: string;
  errorMessage?: string | null;
};

const app = document.querySelector<HTMLDivElement>('#app')!;

app.innerHTML = `
  <main class="dashboard">
    <header class="topbar">
      <div>
        <p class="eyebrow">Workflow system</p>
        <h1>PlayBook operational dashboard</h1>
      </div>
      <button id="refreshButton" class="primary">Refresh</button>
    </header>

    <section class="cards">
      <article class="card">
        <h2>PlayBooks</h2>
        <div id="playbookList" class="list"></div>
      </article>

      <article class="card">
        <h2>Subscriptions</h2>
        <div id="subscriptionList" class="list"></div>
      </article>
    </section>

    <section class="card workflow-card">
      <h2>Workflow actions</h2>
      <div class="form-grid">
        <label>
          PlayBook
          <select id="playbookSelect"></select>
        </label>

        <label>
          Entity type
          <input id="entityTypeInput" value="Proposal" />
        </label>

        <label>
          Entity ID
          <input id="entityIdInput" placeholder="GUID for Proposal / Opportunity" />
        </label>

        <label class="full-width">
          Payload (JSON)
          <textarea id="payloadInput">{"decision":"Approved"}</textarea>
        </label>

        <div class="button-row">
          <button id="startWorkflowButton" class="primary">Start workflow</button>
        </div>

        <label>
          Execution ID
          <input id="executionIdInput" placeholder="Execution GUID" />
        </label>

        <label>
          Resume decision
          <select id="decisionSelect">
            <option value="Approved">Approved</option>
            <option value="Rejected">Rejected</option>
          </select>
        </label>

        <div class="button-row">
          <button id="resumeWorkflowButton" class="secondary">Resume workflow</button>
        </div>
      </div>

      <div id="workflowStatus" class="status-panel" aria-live="polite"></div>
    </section>
  </main>
`;

const playbookList = document.querySelector<HTMLDivElement>('#playbookList')!;
const subscriptionList = document.querySelector<HTMLDivElement>('#subscriptionList')!;
const refreshButton = document.querySelector<HTMLButtonElement>('#refreshButton')!;
const playbookSelect = document.querySelector<HTMLSelectElement>('#playbookSelect')!;
const entityTypeInput = document.querySelector<HTMLInputElement>('#entityTypeInput')!;
const entityIdInput = document.querySelector<HTMLInputElement>('#entityIdInput')!;
const payloadInput = document.querySelector<HTMLTextAreaElement>('#payloadInput')!;
const executionIdInput = document.querySelector<HTMLInputElement>('#executionIdInput')!;
const decisionSelect = document.querySelector<HTMLSelectElement>('#decisionSelect')!;
const workflowStatus = document.querySelector<HTMLDivElement>('#workflowStatus')!;

const formatStatus = (value: string) => value.replace(/([A-Z])/g, ' $1').trim();

function setWorkflowStatus(message: string, isError = false) {
  workflowStatus.textContent = message;
  workflowStatus.classList.toggle('error', isError);
}

async function loadData() {
  try {
    const [playbooksResponse, subscriptionsResponse] = await Promise.all([
      fetch('http://localhost:5146/api/workflows/playbooks'),
      fetch('http://localhost:5146/api/crm/subscriptions')
    ]);

    if (!playbooksResponse.ok || !subscriptionsResponse.ok) {
      throw new Error('Unable to load workflow data');
    }

    const playbooks = (await playbooksResponse.json()) as PlayBookSummary[];
    const subscriptions = (await subscriptionsResponse.json()) as SubscriptionSummary[];

    playbookList.innerHTML = playbooks.length
      ? playbooks.map((playbook) => `
          <div class="item">
            <div>
              <strong>${playbook.name}</strong>
              <small>v${playbook.version}</small>
            </div>
            <span class="pill ${playbook.status.toLowerCase()}">${formatStatus(playbook.status)}</span>
          </div>
        `).join('')
      : '<p class="empty">No playbooks available.</p>';

    playbookSelect.innerHTML = playbooks.length
      ? playbooks.map((playbook) => `<option value="${playbook.id}">${playbook.name}</option>`).join('')
      : '<option value="">No playbooks</option>';

    subscriptionList.innerHTML = subscriptions.length
      ? subscriptions.map((subscription) => `
          <div class="item">
            <div>
              <strong>Customer ${subscription.customerId.slice(0, 8)}</strong>
              <small>${new Date(subscription.startDate).toLocaleDateString()} → ${new Date(subscription.endDate).toLocaleDateString()}</small>
            </div>
            <span class="pill ${subscription.status.toLowerCase()}">${formatStatus(subscription.status)}</span>
          </div>
        `).join('')
      : '<p class="empty">No subscriptions available.</p>';
  } catch (error) {
    playbookList.innerHTML = '<p class="empty">The API is not responding.</p>';
    subscriptionList.innerHTML = '<p class="empty">The API is not responding.</p>';
    playbookSelect.innerHTML = '<option value="">Unavailable</option>';
    console.error(error);
  }
}

async function startWorkflow() {
  const playBookId = playbookSelect.value;
  const entityType = entityTypeInput.value.trim() || 'Proposal';
  const entityId = entityIdInput.value.trim();

  if (!playBookId || !entityId) {
    setWorkflowStatus('Select a playbook and provide an entity ID before starting a workflow.', true);
    return;
  }

  try {
    const rawPayload = payloadInput.value.trim();
    const parsedPayload = rawPayload ? JSON.parse(rawPayload) : {};

    const response = await fetch('http://localhost:5146/api/workflows/executions', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        playBookId,
        entityType,
        entityId,
        payload: parsedPayload
      })
    });

    const result = (await response.json()) as WorkflowExecution;
    if (!response.ok) {
      throw new Error(result.errorMessage ?? 'Unable to start workflow.');
    }

    executionIdInput.value = result.id;
    setWorkflowStatus(`Workflow started: ${result.status} (${result.id})`);
  } catch (error) {
    setWorkflowStatus(error instanceof Error ? error.message : 'Workflow start failed.', true);
  }
}

async function resumeWorkflow() {
  const executionId = executionIdInput.value.trim();
  if (!executionId) {
    setWorkflowStatus('Provide an execution ID before resuming the workflow.', true);
    return;
  }

  try {
    const response = await fetch(`http://localhost:5146/api/workflows/executions/${executionId}/resume`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        payload: { decision: decisionSelect.value }
      })
    });

    const result = (await response.json()) as WorkflowExecution;
    if (!response.ok) {
      throw new Error(result.errorMessage ?? 'Unable to resume workflow.');
    }

    setWorkflowStatus(`Workflow resumed: ${result.status} (${result.id})`);
  } catch (error) {
    setWorkflowStatus(error instanceof Error ? error.message : 'Workflow resume failed.', true);
  }
}

refreshButton.addEventListener('click', loadData);
document.querySelector<HTMLButtonElement>('#startWorkflowButton')!.addEventListener('click', startWorkflow);
document.querySelector<HTMLButtonElement>('#resumeWorkflowButton')!.addEventListener('click', resumeWorkflow);

loadData();
