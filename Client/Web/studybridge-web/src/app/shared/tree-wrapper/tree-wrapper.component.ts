import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Tree } from 'primeng/tree';
import { TreeNode } from 'primeng/api';

export interface TreeConfig {
  selectionMode?: 'single' | 'multiple' | 'checkbox';
  showHeader?: boolean;
  showCounts?: boolean;
  showControls?: boolean;
  showStats?: boolean;
  headerTitle?: string;
  cssClass?: string;
  expandAll?: boolean;
  minimal?: boolean;
}

export interface TreeCounts {
  totalNodes: number;
  selectedNodes: number;
  parentNodes: number;
  leafNodes: number;
}

@Component({
  selector: 'app-tree-wrapper',
  standalone: true,
  imports: [CommonModule, Tree],
  templateUrl: './tree-wrapper.component.html',
  styleUrl: './tree-wrapper.component.scss'
})
export class TreeWrapperComponent implements OnInit, OnChanges {
  @Input() treeData: TreeNode[] = [];
  @Input() config: TreeConfig = {
    selectionMode: 'checkbox',
    showHeader: true,
    showCounts: true,
    showControls: true,
    showStats: true,
    headerTitle: 'Tree Selection',
    cssClass: 'w-full',
    expandAll: false,
    minimal: false
  };
  @Input() selectedNodes: TreeNode[] = [];

  @Output() selectedNodesChange = new EventEmitter<TreeNode[]>();
  @Output() nodeSelect = new EventEmitter<TreeNode>();
  @Output() nodeUnselect = new EventEmitter<TreeNode>();
  @Output() nodeExpand = new EventEmitter<TreeNode>();
  @Output() nodeCollapse = new EventEmitter<TreeNode>();

  counts: TreeCounts = {
    totalNodes: 0,
    selectedNodes: 0,
    parentNodes: 0,
    leafNodes: 0
  };

  processedTreeData: TreeNode[] = [];
  private expandedNodeKeys: Set<string> = new Set();

  ngOnInit(): void {
    this.initializeTree();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['treeData']) {
      this.initializeTree();
    } else if (changes['selectedNodes']) {
      // Only update counts and labels, don't recreate tree structure
      this.updateCountsOnly();
    }
  }

  private initializeTree(): void {
    // Save current expanded state
    this.saveExpandedState(this.processedTreeData);
    
    this.processedTreeData = this.processTreeData([...this.treeData]);
    this.calculateCounts();
    
    // Restore expanded state or expand all if configured
    if (this.config.expandAll) {
      this.expandAllNodes(this.processedTreeData);
    } else {
      this.restoreExpandedState(this.processedTreeData);
    }
  }

  private updateCountsOnly(): void {
    this.calculateCounts();
    this.updateTreeLabelsPreservingState(this.processedTreeData);
  }

  private processTreeData(nodes: TreeNode[]): TreeNode[] {
    return nodes.map(node => {
      const processedNode = { ...node };
      
      // Store original label for later use
      if (!processedNode.data) {
        processedNode.data = {};
      }
      if (typeof processedNode.data === 'object') {
        (processedNode.data as any).originalLabel = node.label;
      }
      
      if (processedNode.children && processedNode.children.length > 0) {
        processedNode.children = this.processTreeData(processedNode.children);
        
        // Add count to parent node label if showCounts is enabled
        if (this.config.showCounts) {
          const childCount = this.countAllChildren(processedNode);
          const selectedCount = this.countSelectedChildren(processedNode);
          processedNode.label = `${node.label} (${selectedCount}/${childCount})`;
        }
      }
      
      return processedNode;
    });
  }

  private countAllChildren(node: TreeNode): number {
    if (!node.children || node.children.length === 0) {
      return 0;
    }
    
    let count = node.children.length;
    node.children.forEach(child => {
      count += this.countAllChildren(child);
    });
    
    return count;
  }

  private countSelectedChildren(node: TreeNode): number {
    if (!node.children || node.children.length === 0) {
      return 0;
    }
    
    let count = 0;
    node.children.forEach(child => {
      if (this.isNodeSelected(child)) {
        count++;
      }
      count += this.countSelectedChildren(child);
    });
    
    return count;
  }

  private isNodeSelected(node: TreeNode): boolean {
    return this.selectedNodes.some(selected => selected.key === node.key);
  }

  private calculateCounts(): void {
    this.counts.totalNodes = this.countTotalNodes(this.treeData);
    this.counts.selectedNodes = this.selectedNodes.length;
    this.counts.parentNodes = this.countParentNodes(this.treeData);
    this.counts.leafNodes = this.counts.totalNodes - this.counts.parentNodes;
  }

  private countTotalNodes(nodes: TreeNode[]): number {
    let count = 0;
    nodes.forEach(node => {
      count++; // Count current node
      if (node.children && node.children.length > 0) {
        count += this.countTotalNodes(node.children);
      }
    });
    return count;
  }

  private countParentNodes(nodes: TreeNode[]): number {
    let count = 0;
    nodes.forEach(node => {
      if (node.children && node.children.length > 0) {
        count++; // Count parent node
        count += this.countParentNodes(node.children);
      }
    });
    return count;
  }

  private expandAllNodes(nodes: TreeNode[]): void {
    nodes.forEach(node => {
      if (node.children && node.children.length > 0) {
        node.expanded = true;
        this.expandAllNodes(node.children);
      }
    });
  }

  onSelectionChange(event: TreeNode[] | TreeNode | null): void {
    if (Array.isArray(event)) {
      this.selectedNodes = event;
    } else if (event) {
      this.selectedNodes = [event];
    } else {
      this.selectedNodes = [];
    }
    this.selectedNodesChange.emit(this.selectedNodes);
    this.updateCounts();
    // Don't recreate tree, just update labels while preserving expanded state
    this.updateTreeLabelsPreservingState(this.processedTreeData);
  }

  onNodeSelect(event: any): void {
    this.nodeSelect.emit(event.node);
    this.updateCounts();
    this.updateTreeLabelsPreservingState(this.processedTreeData);
  }

  onNodeUnselect(event: any): void {
    this.nodeUnselect.emit(event.node);
    this.updateCounts();
    this.updateTreeLabelsPreservingState(this.processedTreeData);
  }

  onNodeExpand(event: any): void {
    this.expandedNodeKeys.add(event.node.key);
    this.nodeExpand.emit(event.node);
  }

  onNodeCollapse(event: any): void {
    this.expandedNodeKeys.delete(event.node.key);
    this.nodeCollapse.emit(event.node);
  }

  private updateCounts(): void {
    this.counts.selectedNodes = this.selectedNodes.length;
  }

  private updateTreeLabels(): void {
    if (this.config.showCounts) {
      this.updateTreeLabelsPreservingState(this.processedTreeData);
    }
  }

  expandAll(): void {
    this.expandAllNodes(this.processedTreeData);
  }

  collapseAll(): void {
    this.collapseAllNodes(this.processedTreeData);
  }

  private collapseAllNodes(nodes: TreeNode[]): void {
    nodes.forEach(node => {
      if (node.children && node.children.length > 0) {
        node.expanded = false;
        this.collapseAllNodes(node.children);
      }
    });
  }

  selectAll(): void {
    const allNodes = this.getAllNodes(this.treeData);
    this.selectedNodes = [...allNodes];
    this.selectedNodesChange.emit(this.selectedNodes);
    this.updateCounts();
    this.updateTreeLabelsPreservingState(this.processedTreeData);
  }

  clearSelection(): void {
    this.selectedNodes = [];
    this.selectedNodesChange.emit(this.selectedNodes);
    this.updateCounts();
    this.updateTreeLabelsPreservingState(this.processedTreeData);
  }

  private getAllNodes(nodes: TreeNode[]): TreeNode[] {
    let allNodes: TreeNode[] = [];
    nodes.forEach(node => {
      allNodes.push(node);
      if (node.children && node.children.length > 0) {
        allNodes = allNodes.concat(this.getAllNodes(node.children));
      }
    });
    return allNodes;
  }

  private saveExpandedState(nodes: TreeNode[]): void {
    nodes.forEach(node => {
      if (node.expanded && node.key) {
        this.expandedNodeKeys.add(node.key);
      }
      if (node.children && node.children.length > 0) {
        this.saveExpandedState(node.children);
      }
    });
  }

  private restoreExpandedState(nodes: TreeNode[]): void {
    nodes.forEach(node => {
      if (node.key && this.expandedNodeKeys.has(node.key)) {
        node.expanded = true;
      }
      if (node.children && node.children.length > 0) {
        this.restoreExpandedState(node.children);
      }
    });
  }

  private updateTreeLabelsPreservingState(nodes: TreeNode[]): void {
    nodes.forEach(node => {
      if (node.children && node.children.length > 0) {
        // Update parent node label with current counts
        if (this.config.showCounts) {
          const originalLabel = this.getOriginalLabel(node);
          const childCount = this.countAllChildren(node);
          const selectedCount = this.countSelectedChildren(node);
          node.label = `${originalLabel} (${selectedCount}/${childCount})`;
        }
        
        // Recursively update children
        this.updateTreeLabelsPreservingState(node.children);
      }
    });
  }

  private getOriginalLabel(node: TreeNode): string {
    // Try to get original label from stored data first
    if (node.data && typeof node.data === 'object' && (node.data as any).originalLabel) {
      return (node.data as any).originalLabel;
    }
    
    // Fallback: extract original label by removing count suffix if present
    const label = node.label || '';
    const countPattern = /\s*\(\d+\/\d+\)$/;
    return label.replace(countPattern, '');
  }
}